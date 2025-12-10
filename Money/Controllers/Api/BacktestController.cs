using Microsoft.AspNetCore.Mvc;
using Money.Models;
using Money.Services;

namespace Money.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class BacktestController : ControllerBase
{
    private readonly BacktestService _backtestService;
    private readonly ILogger<BacktestController> _logger;

    public BacktestController(BacktestService backtestService, ILogger<BacktestController> logger)
    {
        _backtestService = backtestService;
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
}
