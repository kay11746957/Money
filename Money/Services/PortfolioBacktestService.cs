using Money.Models;

namespace Money.Services;

public class PortfolioBacktestService
{
    private readonly YahooFinanceService _yahooService;
    private readonly ILogger<PortfolioBacktestService> _logger;
    private const decimal RiskFreeRate = 0.04m; // 4% 無風險利率 (10年期美債)

    public PortfolioBacktestService(YahooFinanceService yahooService, ILogger<PortfolioBacktestService> logger)
    {
        _yahooService = yahooService;
        _logger = logger;
    }

    public async Task<PortfolioBacktestResult> CalculateAsync(PortfolioBacktestRequest request)
    {
        var result = new PortfolioBacktestResult();
        var allHistoricalData = new Dictionary<string, EtfHistoricalData>();

        // 抓取所有 ETF 資料
        foreach (var item in request.PortfolioItems.Where(i => !string.IsNullOrEmpty(i.Symbol)))
        {
            var data = await _yahooService.GetHistoricalDataAsync(item.Symbol, request.Period);
            if (data != null && data.Prices.Count > 0)
            {
                allHistoricalData[item.Symbol] = data;
            }
        }

        if (allHistoricalData.Count == 0)
        {
            _logger.LogWarning("無法取得任何 ETF 資料");
            return result;
        }

        // 計算個別 ETF 結果
        foreach (var item in request.PortfolioItems.Where(i => allHistoricalData.ContainsKey(i.Symbol)))
        {
            var individualResult = CalculateIndividualEtf(
                allHistoricalData[item.Symbol], 
                request, 
                item.Weight);
            result.IndividualResults.Add(individualResult);
        }

        // 計算組合整體結果
        result.Portfolio = CalculatePortfolioResult(
            allHistoricalData, 
            request.PortfolioItems.Where(i => allHistoricalData.ContainsKey(i.Symbol)).ToList(),
            request);

        // 計算比較指標
        result.MetricComparisons = GenerateMetricComparisons(result.Portfolio, result.IndividualResults);

        return result;
    }

    private EtfBacktestResult CalculateIndividualEtf(EtfHistoricalData data, PortfolioBacktestRequest request, decimal weight)
    {
        var prices = data.Prices.OrderBy(p => p.Date).ToList();
        var performanceData = new List<PerformancePoint>();
        var monthlyReturns = new List<decimal>();
        
        decimal totalShares = 0;
        decimal totalInvested = 0;
        decimal peakValue = 0;
        decimal maxDrawdown = 0;
        decimal? prevPrice = null;

        foreach (var price in prices)
        {
            if (request.InvestmentMode == "dca")
            {
                var shares = request.Amount / price.AdjustedClose;
                totalShares += shares;
                totalInvested += request.Amount;
            }
            else if (totalShares == 0)
            {
                totalShares = request.Amount / price.AdjustedClose;
                totalInvested = request.Amount;
            }

            var currentValue = totalShares * price.AdjustedClose;
            var cumulativeReturn = totalInvested > 0 
                ? (currentValue - totalInvested) / totalInvested * 100 
                : 0;

            performanceData.Add(new PerformancePoint
            {
                Date = price.Date,
                Value = currentValue,
                CumulativeReturn = cumulativeReturn
            });

            // 計算月報酬率
            if (prevPrice.HasValue && prevPrice.Value > 0)
            {
                monthlyReturns.Add((price.AdjustedClose - prevPrice.Value) / prevPrice.Value);
            }
            prevPrice = price.AdjustedClose;

            // 計算最大回撤
            if (currentValue > peakValue) peakValue = currentValue;
            var drawdown = peakValue > 0 ? (peakValue - currentValue) / peakValue * 100 : 0;
            if (drawdown > maxDrawdown) maxDrawdown = drawdown;
        }

        var finalValue = performanceData.LastOrDefault()?.Value ?? 0;
        var totalReturn = finalValue - totalInvested;
        var totalReturnPercent = totalInvested > 0 ? totalReturn / totalInvested * 100 : 0;
        var years = request.Period;
        var cagr = years > 0 && totalInvested > 0
            ? (decimal)(Math.Pow((double)(finalValue / totalInvested), 1.0 / years) - 1) * 100
            : 0;

        // 計算標準差與年化波動率
        var stdDev = CalculateStandardDeviation(monthlyReturns);
        var annualizedVol = stdDev * (decimal)Math.Sqrt(12);
        
        // 計算平均報酬
        var avgReturn = monthlyReturns.Count > 0 ? monthlyReturns.Average() : 0;
        var annualizedReturn = avgReturn * 12;
        
        // 計算夏普比率
        var sharpe = annualizedVol > 0 
            ? (annualizedReturn - RiskFreeRate) / annualizedVol 
            : 0;
        
        // 計算索提諾比率 (只考慮下跌)
        var downReturns = monthlyReturns.Where(r => r < 0).ToList();
        var downDev = CalculateStandardDeviation(downReturns) * (decimal)Math.Sqrt(12);
        var sortino = downDev > 0 
            ? (annualizedReturn - RiskFreeRate) / downDev 
            : 0;
        
        // Beta (簡化估算：股票 ETF 約 1.0，債券 ETF 約 0.2)
        var beta = data.Symbol.Contains("B") || data.Symbol.Contains("BND") || data.Symbol.Contains("TLT")
            ? 0.2m  // 債券 ETF
            : 1.0m; // 股票 ETF

        return new EtfBacktestResult
        {
            Symbol = data.Symbol,
            Name = data.Name,
            TotalInvested = Math.Round(totalInvested, 0),
            FinalValue = Math.Round(finalValue, 0),
            TotalReturn = Math.Round(totalReturn, 0),
            TotalReturnPercent = Math.Round(totalReturnPercent, 2),
            Cagr = Math.Round(cagr, 2),
            MaxDrawdown = Math.Round(maxDrawdown, 2),
            StandardDeviation = Math.Round(stdDev * 100, 2),
            AnnualizedVolatility = Math.Round(annualizedVol * 100, 2),
            SharpeRatio = Math.Round(sharpe, 2),
            SortinoRatio = Math.Round(sortino, 2),
            Beta = Math.Round(beta, 2),
            PerformanceData = performanceData
        };
    }

    private PortfolioSummary CalculatePortfolioResult(
        Dictionary<string, EtfHistoricalData> allData,
        List<PortfolioItem> items,
        PortfolioBacktestRequest request)
    {
        // 使用月份匹配（因為台股和美股交易日不同）
        // 取得每個 ETF 每月最後一個交易日的價格
        var monthlyPrices = new Dictionary<string, Dictionary<string, decimal>>();
        
        foreach (var kvp in allData)
        {
            var symbol = kvp.Key;
            var prices = kvp.Value.Prices;
            
            monthlyPrices[symbol] = prices
                .GroupBy(p => $"{p.Date.Year}-{p.Date.Month:D2}")
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.Date).First().AdjustedClose
                );
        }
        
        // 找出共同的月份
        var commonMonths = monthlyPrices.Values
            .Select(d => d.Keys.ToHashSet())
            .Aggregate((a, b) => a.Intersect(b).ToHashSet())
            .OrderBy(m => m)
            .ToList();

        if (commonMonths.Count == 0)
        {
            _logger.LogWarning("無共同月份資料，ETFs: {Symbols}", string.Join(", ", items.Select(i => i.Symbol)));
            return new PortfolioSummary();
        }
        
        _logger.LogInformation("找到 {Count} 個共同月份", commonMonths.Count);

        var performanceData = new List<PerformancePoint>();
        var monthlyReturns = new List<decimal>();
        
        // 追蹤每個 ETF 的持股數量
        var shares = items.ToDictionary(i => i.Symbol, i => 0m);
        decimal totalInvested = 0;
        decimal peakValue = 0;
        decimal maxDrawdown = 0;
        decimal? prevPortfolioValue = null;

        foreach (var month in commonMonths)
        {
            // DCA：每月按權重投入
            if (request.InvestmentMode == "dca")
            {
                foreach (var item in items)
                {
                    var price = monthlyPrices[item.Symbol][month];
                    var investAmount = request.Amount * (item.Weight / 100m);
                    var newShares = investAmount / price;
                    shares[item.Symbol] += newShares;
                }
                totalInvested += request.Amount;
            }
            else if (totalInvested == 0)
            {
                // 單筆投入
                foreach (var item in items)
                {
                    var price = monthlyPrices[item.Symbol][month];
                    var investAmount = request.Amount * (item.Weight / 100m);
                    shares[item.Symbol] = investAmount / price;
                }
                totalInvested = request.Amount;
            }

            // 計算當前投資組合總值
            decimal portfolioValue = 0;
            foreach (var item in items)
            {
                var price = monthlyPrices[item.Symbol][month];
                portfolioValue += shares[item.Symbol] * price;
            }

            // 計算累積報酬率
            var cumulativeReturn = totalInvested > 0 
                ? (portfolioValue - totalInvested) / totalInvested * 100 
                : 0;

            // 將月份字串轉換為日期
            var dateParts = month.Split('-');
            var date = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), 1);

            performanceData.Add(new PerformancePoint
            {
                Date = date,
                Value = portfolioValue,
                CumulativeReturn = cumulativeReturn
            });

            // 計算月報酬率 (使用加權價格報酬，排除新資金影響)
            // 在 DCA 模式下，計算「本月投入前」的組合價值變化
            if (prevPortfolioValue.HasValue && prevPortfolioValue.Value > 0)
            {
                // 計算加權平均價格報酬率 (使用各 ETF 的價格變化)
                decimal weightedPriceReturn = 0;
                foreach (var item in items)
                {
                    var currentPrice = monthlyPrices[item.Symbol][month];
                    var prevMonth = commonMonths[commonMonths.IndexOf(month) - 1];
                    var prevPrice = monthlyPrices[item.Symbol][prevMonth];
                    if (prevPrice > 0)
                    {
                        var priceReturn = (currentPrice - prevPrice) / prevPrice;
                        weightedPriceReturn += priceReturn * (item.Weight / 100m);
                    }
                }
                monthlyReturns.Add(weightedPriceReturn);
            }
            prevPortfolioValue = portfolioValue;

            // 計算最大回撤
            if (portfolioValue > peakValue) peakValue = portfolioValue;
            var drawdown = peakValue > 0 ? (peakValue - portfolioValue) / peakValue * 100 : 0;
            if (drawdown > maxDrawdown) maxDrawdown = drawdown;
        }

        // 計算最終值
        var finalValue = performanceData.LastOrDefault()?.Value ?? 0;
        var totalReturnPercent = totalInvested > 0 
            ? (finalValue - totalInvested) / totalInvested * 100 
            : 0;

        // 計算年化報酬率 (CAGR)
        var years = request.Period > 0 ? request.Period : 10;
        var cagr = years > 0 && totalInvested > 0 && finalValue > 0
            ? (decimal)(Math.Pow((double)(finalValue / totalInvested), 1.0 / years) - 1) * 100
            : 0;

        // 計算風險指標
        var stdDev = CalculateStandardDeviation(monthlyReturns);
        var annualizedVol = stdDev * (decimal)Math.Sqrt(12);
        var avgMonthlyReturn = monthlyReturns.Count > 0 ? monthlyReturns.Average() : 0;
        var annualizedReturn = avgMonthlyReturn * 12;
        
        // 夏普比率
        var sharpe = annualizedVol > 0 
            ? (annualizedReturn - RiskFreeRate) / annualizedVol 
            : 0;

        // 索提諾比率 (只考慮下跌)
        var downReturns = monthlyReturns.Where(r => r < 0).ToList();
        var downDev = CalculateStandardDeviation(downReturns) * (decimal)Math.Sqrt(12);
        var sortino = downDev > 0 
            ? (annualizedReturn - RiskFreeRate) / downDev 
            : 0;

        // Beta (根據組合內容估算)
        var stockWeight = items.Where(i => !i.Symbol.Contains("B") && !i.Symbol.Contains("TLT")).Sum(i => i.Weight);
        var beta = stockWeight / 100m; // 簡化估算

        // 崔納值
        var treynor = beta > 0 
            ? (annualizedReturn - RiskFreeRate) / beta 
            : 0;

        var allocation = string.Join(" + ", items.Select(i => $"{i.Symbol} {i.Weight}%"));

        return new PortfolioSummary
        {
            Name = "我的組合",
            Allocation = allocation,
            TotalInvested = Math.Round(totalInvested, 0),
            FinalValue = Math.Round(finalValue, 0),
            TotalReturnPercent = Math.Round(totalReturnPercent, 2),
            Cagr = Math.Round(cagr, 2),
            StandardDeviation = Math.Round(stdDev * 100, 2),
            AnnualizedVolatility = Math.Round(annualizedVol * 100, 2),
            MaxDrawdown = Math.Round(maxDrawdown, 2),
            Beta = Math.Round(beta, 2),
            SharpeRatio = Math.Round(sharpe, 2),
            SortinoRatio = Math.Round(sortino, 2),
            TreynorRatio = Math.Round(treynor * 100, 2),
            PerformanceData = performanceData,
            Contributions = CalculateContributions(allData, items, shares, finalValue)
        };
    }

    private List<PortfolioContribution> CalculateContributions(
        Dictionary<string, EtfHistoricalData> allData,
        List<PortfolioItem> items,
        Dictionary<string, decimal> shares,
        decimal totalValue)
    {
        return items.Select(item => {
            var lastPrice = allData.ContainsKey(item.Symbol) 
                ? allData[item.Symbol].Prices.LastOrDefault()?.AdjustedClose ?? 0 
                : 0;
            var currentValue = shares[item.Symbol] * lastPrice;
            var contribution = totalValue > 0 ? currentValue / totalValue * 100 : 0;
            
            return new PortfolioContribution
            {
                Symbol = item.Symbol,
                Name = allData.ContainsKey(item.Symbol) ? allData[item.Symbol].Name : item.Symbol,
                Weight = item.Weight,
                ContributionToReturn = Math.Round(contribution, 2)
            };
        }).ToList();
    }

    private List<MetricComparison> GenerateMetricComparisons(PortfolioSummary portfolio, List<EtfBacktestResult> individuals)
    {
        var comparisons = new List<MetricComparison>();

        // CAGR 比較
        var cagrValues = individuals.ToDictionary(i => i.Symbol, i => i.Cagr);
        comparisons.Add(new MetricComparison
        {
            MetricName = "年化報酬率",
            MetricDescription = "每年平均複合報酬率",
            PortfolioValue = portfolio.Cagr,
            IndividualValues = cagrValues,
            Advantage = "介於個別 ETF 之間",
            IsPortfolioBetter = false
        });

        // 年化波動率比較
        var volValues = individuals.ToDictionary(i => i.Symbol, i => i.AnnualizedVolatility);
        var portfolioBetterVol = portfolio.AnnualizedVolatility < volValues.Values.Max();
        comparisons.Add(new MetricComparison
        {
            MetricName = "年化波動率",
            MetricDescription = "報酬率的年化標準差，衡量風險",
            PortfolioValue = portfolio.AnnualizedVolatility,
            IndividualValues = volValues,
            Advantage = portfolioBetterVol ? "✅ 分散投資降低風險" : "",
            IsPortfolioBetter = portfolioBetterVol
        });

        // 最大回撤比較
        var drawdownValues = individuals.ToDictionary(i => i.Symbol, i => i.MaxDrawdown);
        var portfolioBetterDrawdown = portfolio.MaxDrawdown < drawdownValues.Values.Max();
        comparisons.Add(new MetricComparison
        {
            MetricName = "最大回撤",
            MetricDescription = "從高點到低點的最大跌幅",
            PortfolioValue = portfolio.MaxDrawdown,
            IndividualValues = drawdownValues,
            Advantage = portfolioBetterDrawdown ? "✅ 抗跌能力較強" : "",
            IsPortfolioBetter = portfolioBetterDrawdown
        });

        // 夏普比率比較
        var sharpeValues = individuals.ToDictionary(i => i.Symbol, i => i.SharpeRatio);
        var portfolioBetterSharpe = portfolio.SharpeRatio > sharpeValues.Values.Max();
        comparisons.Add(new MetricComparison
        {
            MetricName = "夏普比率",
            MetricDescription = "每單位風險的超額報酬",
            PortfolioValue = portfolio.SharpeRatio,
            IndividualValues = sharpeValues,
            Advantage = portfolioBetterSharpe ? "✅ 風險調整報酬最佳" : "",
            IsPortfolioBetter = portfolioBetterSharpe
        });

        // 索提諾比率比較
        var sortinoValues = individuals.ToDictionary(i => i.Symbol, i => i.SortinoRatio);
        var portfolioBetterSortino = portfolio.SortinoRatio > sortinoValues.Values.Max();
        comparisons.Add(new MetricComparison
        {
            MetricName = "索提諾比率",
            MetricDescription = "只考慮下跌風險的報酬/風險比",
            PortfolioValue = portfolio.SortinoRatio,
            IndividualValues = sortinoValues,
            Advantage = portfolioBetterSortino ? "✅ 下跌風險控制最佳" : "",
            IsPortfolioBetter = portfolioBetterSortino
        });

        // Beta 比較
        var betaValues = individuals.ToDictionary(i => i.Symbol, i => i.Beta);
        var portfolioBetterBeta = portfolio.Beta < 1;
        comparisons.Add(new MetricComparison
        {
            MetricName = "Beta 值",
            MetricDescription = "相對於大盤的敏感度",
            PortfolioValue = portfolio.Beta,
            IndividualValues = betaValues,
            Advantage = portfolioBetterBeta ? "✅ 波動較大盤低" : "",
            IsPortfolioBetter = portfolioBetterBeta
        });

        return comparisons;
    }

    private decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count < 2) return 0;
        
        var avg = values.Average();
        var sumSquares = values.Sum(v => (v - avg) * (v - avg));
        return (decimal)Math.Sqrt((double)(sumSquares / (values.Count - 1)));
    }
}
