
using Microsoft.AspNetCore.Mvc;
using homework3.Services;
namespace homework3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{

    private readonly StockService _stockService;
    private readonly ICustomLogger _logger;

    public StocksController(StockService service, ICustomLogger logger) 
    {
        _stockService = service;
        _logger = logger;
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetPrice(string symbol)
    {
        var result = await _stockService.GetStockPriceAsync(symbol);

        if (result == null)
        {
            _logger.LogAction($"Данные для тикера {symbol} не найдены", LogLevels.Warning);
            return NotFound(new { message = $"Акция {symbol} не найдена" });
        }

        else
        {
            return Ok(result);
        }
    }
}

