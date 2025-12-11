using Microsoft.AspNetCore.Mvc;
using Money.Models;
using Money.Services;

namespace Money.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class BacktestController : ControllerBase
{
    private readonly BacktestService _backtestService;
    private readonly PortfolioBacktestService _portfolioService;
    private readonly ILogger<BacktestController> _logger;

    public BacktestController(
        BacktestService backtestService, 
        PortfolioBacktestService portfolioService,
        ILogger<BacktestController> logger)
    {
        _backtestService = backtestService;
        _portfolioService = portfolioService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<BacktestResult>> Calculate([FromBody] BacktestRequest request)
    {
        if (request.Symbols == null || request.Symbols.Count == 0)
        {
            return BadRequest("請至少選擇一檔 ETF");
        }

        if (request.Symbols.Count > 5)
        {
            return BadRequest("最多只能選擇 5 檔 ETF");
        }

        _logger.LogInformation("開始回測: {Symbols}, 期間: {Period}年", 
            string.Join(", ", request.Symbols), request.Period);

        try
        {
            var result = await _backtestService.CalculateAsync(request);
            
            _logger.LogInformation("回測完成: {Count} 檔 ETF", result.Results.Count);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回測計算失敗");
            return StatusCode(500, "回測計算失敗，請稍後再試");
        }
    }

    [HttpPost("portfolio")]
    public async Task<ActionResult<PortfolioBacktestResult>> CalculatePortfolio([FromBody] PortfolioBacktestRequest request)
    {
        if (request.PortfolioItems == null || request.PortfolioItems.Count == 0)
        {
            return BadRequest("請至少設定一檔 ETF");
        }

        var totalWeight = request.PortfolioItems.Sum(i => i.Weight);
        if (Math.Abs(totalWeight - 100) > 0.01m)
        {
            return BadRequest($"配置比例總和必須為 100%，目前為 {totalWeight}%");
        }

        _logger.LogInformation("開始投資組合回測: {Items}, 期間: {Period}年", 
            string.Join(", ", request.PortfolioItems.Select(i => $"{i.Symbol}({i.Weight}%)")), 
            request.Period);

        try
        {
            var result = await _portfolioService.CalculateAsync(request);
            
            _logger.LogInformation("投資組合回測完成");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "投資組合回測失敗");
            return StatusCode(500, "回測計算失敗，請稍後再試");
        }
    }
}

