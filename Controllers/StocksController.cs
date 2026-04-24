
using Microsoft.AspNetCore.Mvc;
using homework3.Services;
using homework3.Models;

namespace homework3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    // Объяви приватные readonly поля для сервиса и логгера
    public readonly StockService _stockService;
    public readonly ICustomLogger _logger;

    // Создай конструктор и внедри зависимости (DI)
    public StocksController(StockService service, ICustomLogger logger) 
    {
        _stockService = service;
        _logger = logger;
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetPrice(string symbol)
    {
        // 1. Вызови асинхронный метод сервиса GetStockPriceAsync(symbol)
        
        // 2. Сделай проверку: если результат null -> верни NotFound()
        
        // 3. Если данные есть -> верни Ok(result)
        return default; // замени на свою логику
    }
}

