using Microsoft.AspNetCore.Mvc;
using Money.Models;
using Money.Services;

namespace Money.Controllers;

public class CalculatorController : Controller
{
    private readonly DcaCalculatorService _calculatorService;

    public CalculatorController(DcaCalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Dca");
    }

    public IActionResult Dca()
    {
        return View();
    }

    public IActionResult Fire()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Calculate([FromBody] DcaInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = _calculatorService.Calculate(input);
        return Json(result);
    }
}
