using Microsoft.AspNetCore.Mvc;
using Money.Models;
using Money.Services;
using System.Diagnostics;

namespace Money.Controllers;

public class HomeController : Controller
{
    private readonly DcaCalculatorService _calculatorService;

    public HomeController(DcaCalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }

    public IActionResult Index()
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
