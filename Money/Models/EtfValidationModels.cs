namespace Money.Models;

public class EtfValidationRequest
{
    public string Symbol { get; set; } = string.Empty;
}

public class EtfValidationResult
{
    public bool IsValid { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "ETF", "EQUITY", "MUTUALFUND", etc.
    public string Market { get; set; } = string.Empty; // "US", "TW", "HK"
    public string Message { get; set; } = string.Empty;
    public EtfDetailInfo? Details { get; set; }
}

public class EtfDetailInfo
{
    public string FullName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public decimal? RegularMarketPrice { get; set; }
}

public class EtfSuggestion
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Market { get; set; } = string.Empty;
}

public class YahooQuoteResponse
{
    public string Symbol { get; set; } = string.Empty;
    public string LongName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string QuoteType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public decimal? RegularMarketPrice { get; set; }
}
