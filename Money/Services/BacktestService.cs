using Money.Models;

namespace Money.Services;

public class BacktestService
{
    private readonly YahooFinanceService _yahooService;
    private readonly ILogger<BacktestService> _logger;

    public BacktestService(YahooFinanceService yahooService, ILogger<BacktestService> logger)
    {
        _yahooService = yahooService;
        _logger = logger;
    }

    public async Task<BacktestResult> CalculateAsync(BacktestRequest request)
    {
        var result = new BacktestResult();

        foreach (var symbol in request.Symbols)
        {
            var historicalData = await _yahooService.GetHistoricalDataAsync(symbol, request.Period);
            
            if (historicalData == null || historicalData.Prices.Count < 2)
            {
                _logger.LogWarning("無法取得 {Symbol} 的歷史資料", symbol);
                continue;
            }

            var etfResult = CalculateEtfBacktest(historicalData, request);
            result.Results.Add(etfResult);
        }

        return result;
    }

    private EtfBacktestResult CalculateEtfBacktest(EtfHistoricalData data, BacktestRequest request)
    {
        var prices = data.Prices.OrderBy(p => p.Date).ToList();
        var performanceData = new List<PerformancePoint>();
        
        decimal totalShares = 0;
        decimal totalInvested = 0;
        decimal peakValue = 0;
        decimal maxDrawdown = 0;

        if (request.InvestmentMode == "dca")
        {
            // 定期定額
            foreach (var price in prices)
            {
                var shares = request.Amount / price.AdjustedClose;
                totalShares += shares;
                totalInvested += request.Amount;

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

                // 計算最大回撤
                if (currentValue > peakValue)
                {
                    peakValue = currentValue;
                }
                var drawdown = peakValue > 0 ? (peakValue - currentValue) / peakValue * 100 : 0;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                }
            }
        }
        else
        {
            // 單筆投入
            if (prices.Count > 0)
            {
                var initialPrice = prices[0].AdjustedClose;
                totalShares = request.Amount / initialPrice;
                totalInvested = request.Amount;

                foreach (var price in prices)
                {
                    var currentValue = totalShares * price.AdjustedClose;
                    var cumulativeReturn = (currentValue - totalInvested) / totalInvested * 100;

                    performanceData.Add(new PerformancePoint
                    {
                        Date = price.Date,
                        Value = currentValue,
                        CumulativeReturn = cumulativeReturn
                    });

                    // 計算最大回撤
                    if (currentValue > peakValue)
                    {
                        peakValue = currentValue;
                    }
                    var drawdown = peakValue > 0 ? (peakValue - currentValue) / peakValue * 100 : 0;
                    if (drawdown > maxDrawdown)
                    {
                        maxDrawdown = drawdown;
                    }
                }
            }
        }

        var finalValue = performanceData.LastOrDefault()?.Value ?? 0;
        var totalReturn = finalValue - totalInvested;
        var totalReturnPercent = totalInvested > 0 ? totalReturn / totalInvested * 100 : 0;

        // 計算年化報酬率 (CAGR)
        var years = request.Period;
        var cagr = years > 0 && totalInvested > 0
            ? (decimal)(Math.Pow((double)(finalValue / totalInvested), 1.0 / years) - 1) * 100
            : 0;

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
            PerformanceData = performanceData
        };
    }
}
