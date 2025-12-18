using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Money.Models;

namespace Money.Services;

public class YahooFinanceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<YahooFinanceService> _logger;
    private const int CacheDurationHours = 24;

    // ETF 資訊對照表
    private static readonly Dictionary<string, (string YahooSymbol, string Name, string Market)> EtfInfo = new()
    {
        // 台股 ETF
        { "0050", ("0050.TW", "元大台灣50", "TW") },
        { "0056", ("0056.TW", "元大高股息", "TW") },
        { "006208", ("006208.TW", "富邦台50", "TW") },
        { "00878", ("00878.TW", "國泰永續高股息", "TW") },
        { "00692", ("00692.TW", "富邦公司治理", "TW") },
        { "00679B", ("00679B.TW", "元大美債20年", "TW") },
        
        // 美股 ETF
        { "VTI", ("VTI", "Vanguard 全美股市", "US") },
        { "VOO", ("VOO", "Vanguard S&P 500", "US") },
        { "VT", ("VT", "Vanguard 全世界股市", "US") },
        { "QQQ", ("QQQ", "Invesco 納斯達克100", "US") },
        { "VWO", ("VWO", "Vanguard 新興市場", "US") },
        { "VEA", ("VEA", "Vanguard 已開發市場", "US") },
        { "BND", ("BND", "Vanguard 美國總債券", "US") },
    };

    public YahooFinanceService(HttpClient httpClient, IMemoryCache cache, ILogger<YahooFinanceService> logger)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _cache = cache;
        _logger = logger;
    }

    public async Task<EtfHistoricalData?> GetHistoricalDataAsync(string symbol, int years)
    {
        var cacheKey = $"etf_{symbol}_{years}y";
        
        if (_cache.TryGetValue(cacheKey, out EtfHistoricalData? cachedData))
        {
            _logger.LogInformation("快取命中: {Symbol}", symbol);
            return cachedData;
        }

        try
        {
            if (!EtfInfo.TryGetValue(symbol, out var info))
            {
                _logger.LogWarning("找不到 ETF 資訊: {Symbol}", symbol);
                return null;
            }

            var yahooSymbol = info.YahooSymbol;
            var endDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var startDate = DateTimeOffset.UtcNow.AddYears(-years).ToUnixTimeSeconds();

            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{yahooSymbol}?period1={startDate}&period2={endDate}&interval=1mo";
            
            _logger.LogInformation("抓取 Yahoo Finance 資料: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Yahoo Finance API 回傳錯誤: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = ParseYahooResponse(json, symbol, info.Name);

            if (data != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(CacheDurationHours))
                    .SetPriority(CacheItemPriority.Normal);
                
                _cache.Set(cacheKey, data, cacheOptions);
                _logger.LogInformation("資料已快取: {Symbol}, {Count} 筆", symbol, data.Prices.Count);
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "抓取 ETF 資料失敗: {Symbol}", symbol);
            return null;
        }
    }

    /// <summary>
    /// 驗證 ETF/股票代碼是否有效
    /// </summary>
    public async Task<EtfValidationResult> ValidateSymbolAsync(string symbol)
    {
        try
        {
            // 正規化代碼 (移除空白、轉大寫)
            symbol = symbol.Trim().ToUpper();

            // 檢查是否在已知的 ETF 清單中
            if (EtfInfo.TryGetValue(symbol, out var info))
            {
                return new EtfValidationResult
                {
                    IsValid = true,
                    Symbol = symbol,
                    Name = info.Name,
                    Type = "ETF",
                    Market = info.Market,
                    Message = "代碼有效"
                };
            }

            // 嘗試從 Yahoo Finance 查詢
            var yahooSymbol = DetermineYahooSymbol(symbol);
            var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={yahooSymbol}";
            
            _logger.LogInformation("驗證代碼: {Symbol} (Yahoo: {YahooSymbol})", symbol, yahooSymbol);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return new EtfValidationResult
                {
                    IsValid = false,
                    Symbol = symbol,
                    Message = "無法驗證代碼，請確認代碼是否正確"
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return ParseValidationResponse(json, symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證代碼失敗: {Symbol}", symbol);
            return new EtfValidationResult
            {
                IsValid = false,
                Symbol = symbol,
                Message = "驗證過程發生錯誤"
            };
        }
    }

    /// <summary>
    /// 決定 Yahoo Finance 的代碼格式
    /// </summary>
    private string DetermineYahooSymbol(string symbol)
    {
        // 台股代碼 (純數字)
        if (System.Text.RegularExpressions.Regex.IsMatch(symbol, @"^\d+B?$"))
        {
            return $"{symbol}.TW";
        }
        
        // 美股代碼 (字母)
        return symbol;
    }

    /// <summary>
    /// 解析 Yahoo Finance 驗證回應
    /// </summary>
    private EtfValidationResult ParseValidationResponse(string json, string originalSymbol)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var quoteResponse = root.GetProperty("quoteResponse");
            var result = quoteResponse.GetProperty("result");

            if (result.GetArrayLength() == 0)
            {
                return new EtfValidationResult
                {
                    IsValid = false,
                    Symbol = originalSymbol,
                    Message = "找不到此代碼"
                };
            }

            var quote = result[0];
            var quoteType = quote.TryGetProperty("quoteType", out var qt) ? qt.GetString() ?? "" : "";
            var longName = quote.TryGetProperty("longName", out var ln) ? ln.GetString() ?? "" : "";
            var shortName = quote.TryGetProperty("shortName", out var sn) ? sn.GetString() ?? "" : "";
            var currency = quote.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "" : "";
            var exchange = quote.TryGetProperty("exchange", out var ex) ? ex.GetString() ?? "" : "";
            
            decimal? price = null;
            if (quote.TryGetProperty("regularMarketPrice", out var rmp) && rmp.ValueKind != JsonValueKind.Null)
            {
                price = rmp.GetDecimal();
            }

            // 判斷市場
            var market = exchange.Contains("TAI") || exchange.Contains("TPE") ? "TW" : "US";

            return new EtfValidationResult
            {
                IsValid = true,
                Symbol = originalSymbol,
                Name = !string.IsNullOrEmpty(longName) ? longName : shortName,
                Type = quoteType,
                Market = market,
                Message = "代碼有效",
                Details = new EtfDetailInfo
                {
                    FullName = longName,
                    Currency = currency,
                    Exchange = exchange,
                    RegularMarketPrice = price
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析驗證回應失敗");
            return new EtfValidationResult
            {
                IsValid = false,
                Symbol = originalSymbol,
                Message = "解析回應失敗"
            };
        }
    }

    private EtfHistoricalData? ParseYahooResponse(string json, string symbol, string name)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var chart = root.GetProperty("chart");
            var result = chart.GetProperty("result")[0];
            
            var timestamps = result.GetProperty("timestamp");
            var indicators = result.GetProperty("indicators");
            var adjclose = indicators.GetProperty("adjclose")[0].GetProperty("adjclose");

            var data = new EtfHistoricalData
            {
                Symbol = symbol,
                Name = name,
                Prices = new List<EtfPriceData>()
            };

            for (int i = 0; i < timestamps.GetArrayLength(); i++)
            {
                var timestamp = timestamps[i].GetInt64();
                var price = adjclose[i];
                
                if (price.ValueKind != JsonValueKind.Null)
                {
                    data.Prices.Add(new EtfPriceData
                    {
                        Date = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
                        AdjustedClose = price.GetDecimal()
                    });
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析 Yahoo Finance 回應失敗");
            return null;
        }
    }
}
