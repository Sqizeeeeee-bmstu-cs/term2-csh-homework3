
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using homework3.Services;
using homework3.Exceptions;
using homework3.Data;
using homework3.Models;


namespace homework3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{

    private readonly StockService _stockService;
    private readonly ICustomLogger _logger;
    private readonly AppDbContext _context;

    public StocksController(StockService service, ICustomLogger logger, AppDbContext context) 
    {
        _stockService = service;
        _logger = logger;
        _context = context;
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetPrice(string symbol)
    {
        try 
        {

            if (string.IsNullOrEmpty(symbol) || symbol.Length > 5 || !symbol.All(char.IsLetter))
            {
                throw new ValidationException($"Тикер {symbol} некорректен");
            }

            var result = await _stockService.GetStockPriceAsync(symbol);

            if (result == null)
            {
                return NotFound(new { message = $"Акция {symbol} не найдена" });
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogAction(ex.Message, LogLevels.Warning);
            return BadRequest(new { error = ex.Message });
        }
        catch (ApiException ex)
        {
            _logger.LogAction(ex.Message, LogLevels.Error);
            return StatusCode(502, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogAction($"Непредвиденная ошибка: {ex.Message}", LogLevels.Error);
            return StatusCode(500, "Произошла внутренняя ошибка сервера");
        }
    }

    public class BuyRequest 
    { 
        public string Ticker { get; set; } = string.Empty; 
        public int Quantity { get; set; } 
    }

    [HttpPost("buy")]
    public async Task<IActionResult> BuyStock([FromBody] BuyRequest request)
    {
        try 
        {
            var priceData = await _stockService.GetStockPriceAsync(request.Ticker);

            if (priceData == null)
            {
                return NotFound();
            }

            PortfolioItem item = new PortfolioItem();

            item.Ticker = request.Ticker;
            item.Quantity = request.Quantity;
            item.BuyPrice = priceData.CurrentPrice;
            item.PurchaseDate = DateTime.Now;

            _context.PortfolioItems.Add(item);

            await _context.SaveChangesAsync();

            _logger.LogAction($"В портфель добавлено: {item.Ticker}, цена: {item.BuyPrice}, сколько: {item.Quantity}", LogLevels.Info);

            return Ok(new { message = "Успешно куплено" });
        }
        catch (Exception ex)
        {
            _logger.LogAction($"ошибка {ex}", LogLevels.Error);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio()
    {
        var portfolio = await _context.PortfolioItems.ToListAsync();
        return Ok(portfolio);
    }

}
