using Microsoft.AspNetCore.Mvc;
using Money.Models;
using Money.Services;

namespace Money.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class EtfController : ControllerBase
{
    private readonly YahooFinanceService _yahooFinanceService;
    private readonly EtfSuggestionService _suggestionService;
    private readonly ILogger<EtfController> _logger;

    public EtfController(
        YahooFinanceService yahooFinanceService,
        EtfSuggestionService suggestionService,
        ILogger<EtfController> logger)
    {
        _yahooFinanceService = yahooFinanceService;
        _suggestionService = suggestionService;
        _logger = logger;
    }

    /// <summary>
    /// 驗證 ETF/股票代碼
    /// </summary>
    /// <param name="symbol">ETF/股票代碼</param>
    [HttpGet("validate/{symbol}")]
    public async Task<ActionResult<EtfValidationResult>> ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest(new EtfValidationResult
            {
                IsValid = false,
                Message = "請輸入 ETF 代碼"
            });
        }

        try
        {
            var result = await _yahooFinanceService.ValidateSymbolAsync(symbol);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證 ETF 代碼失敗: {Symbol}", symbol);
            return StatusCode(500, new EtfValidationResult
            {
                IsValid = false,
                Symbol = symbol,
                Message = "驗證過程發生錯誤"
            });
        }
    }

    /// <summary>
    /// 取得所有 ETF 建議
    /// </summary>
    [HttpGet("suggestions")]
    public ActionResult<List<EtfSuggestion>> GetSuggestions()
    {
        var suggestions = _suggestionService.GetAllSuggestions();
        return Ok(suggestions);
    }

    /// <summary>
    /// 依類別取得 ETF 建議
    /// </summary>
    /// <param name="category">類別名稱</param>
    [HttpGet("suggestions/category/{category}")]
    public ActionResult<List<EtfSuggestion>> GetSuggestionsByCategory(string category)
    {
        var suggestions = _suggestionService.GetSuggestionsByCategory(category);
        return Ok(suggestions);
    }

    /// <summary>
    /// 依市場取得 ETF 建議
    /// </summary>
    /// <param name="market">市場代碼 (TW/US)</param>
    [HttpGet("suggestions/market/{market}")]
    public ActionResult<List<EtfSuggestion>> GetSuggestionsByMarket(string market)
    {
        var suggestions = _suggestionService.GetSuggestionsByMarket(market);
        return Ok(suggestions);
    }

    /// <summary>
    /// 取得所有類別
    /// </summary>
    [HttpGet("categories")]
    public ActionResult<List<string>> GetCategories()
    {
        var categories = _suggestionService.GetCategories();
        return Ok(categories);
    }
}
