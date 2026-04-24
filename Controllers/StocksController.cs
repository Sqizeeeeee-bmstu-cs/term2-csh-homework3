
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


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

    [Authorize]
    [HttpPost("buy")]
    public async Task<IActionResult> BuyStock([FromBody] BuyRequest request)
    {
        try 
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Пользователь не идентифицирован" });
            }
            int userId = int.Parse(userIdClaim);

            var priceData = await _stockService.GetStockPriceAsync(request.Ticker);
            if (priceData == null)
            {
                return NotFound(new { message = "Не удалось получить цену акции" });
            }

            PortfolioItem item = new PortfolioItem
            {
                Ticker = request.Ticker.ToUpper(),
                Quantity = request.Quantity,
                BuyPrice = priceData.CurrentPrice,
                PurchaseDate = DateTime.Now,
                UserId = userId
            };

            _context.PortfolioItems.Add(item);
            await _context.SaveChangesAsync();

            _logger.LogAction($"Юзер {userId} купил: {item.Ticker}, цена: {item.BuyPrice}, кол-во: {item.Quantity}", LogLevels.Info);

            return Ok(new { message = "Успешно куплено", id = item.Id });
        }
        catch (Exception ex)
        {
            _logger.LogAction($"Ошибка при покупке: {ex.Message}", LogLevels.Error);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null) return Unauthorized();
        int userId = int.Parse(userIdClaim);

        var portfolio = await _context.PortfolioItems
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return Ok(portfolio);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends()
    {
        var trendSymbols = new[] { "AAPL", "TSLA", "MSFT", "GOOGL", "NVDA", "AMZN" };
        var results = new List<object>();

        foreach (var symbol in trendSymbols)
        {
            var data = await _stockService.GetStockPriceAsync(symbol);
            if (data != null)
            {
                results.Add(new { 
                    Symbol = symbol, 
                    Price = data.CurrentPrice, 
                    Change = data.PercentChange 
                });
            }
        }
        return Ok(results);
    }

}
