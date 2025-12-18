using Money.Models;

namespace Money.Services;

public class EtfSuggestionService
{
    private static readonly List<EtfSuggestion> _suggestions = new()
    {
        // 台股熱門
        new() { Symbol = "0050.TW", Name = "元大台灣50", Category = "台股熱門", Market = "TW" },
        new() { Symbol = "0056.TW", Name = "元大高股息", Category = "台股熱門", Market = "TW" },
        new() { Symbol = "00878.TW", Name = "國泰永續高股息", Category = "台股熱門", Market = "TW" },
        new() { Symbol = "006208.TW", Name = "富邦台50", Category = "台股熱門", Market = "TW" },
        new() { Symbol = "00881.TW", Name = "國泰台灣5G+", Category = "台股熱門", Market = "TW" },
        new() { Symbol = "00679B.TW", Name = "元大美債20年", Category = "台股熱門", Market = "TW" },
        
        // 美股大盤
        new() { Symbol = "VOO", Name = "Vanguard S&P 500", Category = "美股大盤", Market = "US" },
        new() { Symbol = "VTI", Name = "Vanguard 全美股市", Category = "美股大盤", Market = "US" },
        new() { Symbol = "SPY", Name = "SPDR S&P 500", Category = "美股大盤", Market = "US" },
        new() { Symbol = "QQQ", Name = "Invesco 納斯達克100", Category = "美股大盤", Market = "US" },
        
        // 美股債券
        new() { Symbol = "BND", Name = "Vanguard 美國總債券", Category = "美股債券", Market = "US" },
        new() { Symbol = "AGG", Name = "iShares Core 美國綜合債券", Category = "美股債券", Market = "US" },
        new() { Symbol = "TLT", Name = "iShares 20年期美國公債", Category = "美股債券", Market = "US" },
        
        // 國際市場
        new() { Symbol = "VT", Name = "Vanguard 全世界股市", Category = "國際市場", Market = "US" },
        new() { Symbol = "VEA", Name = "Vanguard 已開發市場", Category = "國際市場", Market = "US" },
        new() { Symbol = "VWO", Name = "Vanguard 新興市場", Category = "國際市場", Market = "US" },
    };

    public List<EtfSuggestion> GetAllSuggestions()
    {
        return _suggestions;
    }

    public List<EtfSuggestion> GetSuggestionsByCategory(string category)
    {
        return _suggestions
            .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<EtfSuggestion> GetSuggestionsByMarket(string market)
    {
        return _suggestions
            .Where(s => s.Market.Equals(market, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<string> GetCategories()
    {
        return _suggestions
            .Select(s => s.Category)
            .Distinct()
            .ToList();
    }
}
