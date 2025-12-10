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
    public List<PerformancePoint> PerformanceData { get; set; } = new();
}

public class PerformancePoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal CumulativeReturn { get; set; }
}
