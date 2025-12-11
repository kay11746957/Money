namespace Money.Models;

public class EtfHistoricalData
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<EtfPriceData> Prices { get; set; } = new();
}

public class EtfPriceData
{
    public DateTime Date { get; set; }
    public decimal AdjustedClose { get; set; }
}

public class BacktestRequest
{
    public List<string> Symbols { get; set; } = new();
    public int Period { get; set; } = 10;
    public string InvestmentMode { get; set; } = "dca";
    public decimal Amount { get; set; } = 10000;
    public bool ReinvestDividends { get; set; } = true;
}

public class BacktestResult
{
    public List<EtfBacktestResult> Results { get; set; } = new();
}

public class EtfBacktestResult
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TotalInvested { get; set; }
    public decimal FinalValue { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal TotalReturnPercent { get; set; }
    public decimal Cagr { get; set; }
    public decimal MaxDrawdown { get; set; }
    
    // 進階風險指標
    public decimal StandardDeviation { get; set; }      // 標準差
    public decimal AnnualizedVolatility { get; set; }   // 年化波動率
    public decimal SharpeRatio { get; set; }            // 夏普比率
    public decimal SortinoRatio { get; set; }           // 索提諾比率
    public decimal Beta { get; set; }                   // Beta 值
    
    public List<PerformancePoint> PerformanceData { get; set; } = new();
}

public class PerformancePoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal CumulativeReturn { get; set; }
}

// ===== 投資組合相關 =====

public class PortfolioBacktestRequest
{
    public string BacktestMode { get; set; } = "compare"; // "compare" or "portfolio"
    public List<PortfolioItem> PortfolioItems { get; set; } = new();
    public int Period { get; set; } = 10;
    public string InvestmentMode { get; set; } = "dca";
    public decimal Amount { get; set; } = 10000;
    public bool ReinvestDividends { get; set; } = true;
}

public class PortfolioItem
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Weight { get; set; } = 0;
}

public class PortfolioBacktestResult
{
    // 組合整體結果
    public PortfolioSummary Portfolio { get; set; } = new();
    // 個別 ETF 結果 (用於比較)
    public List<EtfBacktestResult> IndividualResults { get; set; } = new();
    // 組合與個別 ETF 的比較
    public List<MetricComparison> MetricComparisons { get; set; } = new();
}

public class PortfolioSummary
{
    public string Name { get; set; } = "我的組合";
    public string Allocation { get; set; } = string.Empty; // e.g., "VTI 60% + BND 40%"
    public decimal TotalInvested { get; set; }
    public decimal FinalValue { get; set; }
    public decimal TotalReturnPercent { get; set; }
    public decimal Cagr { get; set; }
    
    // 進階風險指標
    public decimal StandardDeviation { get; set; }      // 標準差
    public decimal AnnualizedVolatility { get; set; }   // 年化波動率
    public decimal MaxDrawdown { get; set; }            // 最大回撤
    public decimal Beta { get; set; }                   // Beta 值
    public decimal SharpeRatio { get; set; }            // 夏普比率
    public decimal SortinoRatio { get; set; }           // 索提諾比率
    public decimal TreynorRatio { get; set; }           // 崔納值
    
    public List<PerformancePoint> PerformanceData { get; set; } = new();
    public List<PortfolioContribution> Contributions { get; set; } = new();
}

public class PortfolioContribution
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal IndividualReturn { get; set; }
    public decimal ContributionToReturn { get; set; }
    public decimal IndividualRisk { get; set; }
    public decimal ContributionToRisk { get; set; }
}

public class MetricComparison
{
    public string MetricName { get; set; } = string.Empty;
    public string MetricDescription { get; set; } = string.Empty;
    public decimal PortfolioValue { get; set; }
    public Dictionary<string, decimal> IndividualValues { get; set; } = new();
    public string Advantage { get; set; } = string.Empty; // 組合優勢說明
    public bool IsPortfolioBetter { get; set; }
}

