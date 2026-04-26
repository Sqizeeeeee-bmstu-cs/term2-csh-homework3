
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;
    

    public StocksController(StockService service, ICustomLogger logger, AppDbContext context, IMemoryCache cache) 
    {
        _stockService = service;
        _logger = logger;
        _context = context;
        _cache = cache;
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
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var priceData = await _stockService.GetStockPriceAsync(request.Ticker);
            if (priceData == null) return NotFound("Акция не найдена");

            decimal totalCost = priceData.CurrentPrice * request.Quantity;
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null) return NotFound("Пользователь не найден");
            if (user.Balance < totalCost) 
            {
                return BadRequest($"Недостаточно средств. Нужно: {totalCost}$, на счету: {user.Balance}$");
            }

            user.Balance -= totalCost;

            PortfolioItem item = new PortfolioItem
            {
                Ticker = request.Ticker.ToUpper(),
                Quantity = request.Quantity,
                BuyPrice = priceData.CurrentPrice,
                PurchaseDate = DateTime.Now,
                UserId = userId
            };

            var transaction = new Transaction
            {
                UserId = userId,
                Type = "Buy",
                Ticker = request.Ticker.ToUpper(),
                Amount = -totalCost,
                CreatedAt = DateTime.Now
            };

            _context.PortfolioItems.Add(item);
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            _logger.LogAction($"Юзер {user.Username} купил {request.Quantity} {item.Ticker} за {totalCost}$", LogLevels.Info);

            return Ok(new { 
                message = "Успешно куплено", 
                newBalance = user.Balance,
                totalCost = totalCost 
            });
        }
        catch (Exception ex)
        {
            _logger.LogAction($"Ошибка при покупке: {ex.Message}", LogLevels.Error);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends()
    {
        string cacheKey = "trends_data";

        if (_cache.TryGetValue(cacheKey, out object cachedTrends))
        {
            _logger.LogAction("Тренды отданы из кэша", LogLevels.Info);
            return Ok(cachedTrends);
        }

        var trendSymbols = new[] { "AAPL", "TSLA", "MSFT", "GOOGL", "NVDA", "AMZN" };
        var stockDataList = new List<object>();

        foreach (var symbol in trendSymbols)
        {
            var data = await _stockService.GetStockPriceAsync(symbol);
            if (data != null)
            {
                stockDataList.Add(new { 
                    Symbol = symbol, 
                    Price = data.CurrentPrice, 
                    Change = data.PercentChange 
                });
            }
        }

        var finalResult = new {
            LastUpdated = DateTime.Now.ToString("HH:mm:ss"),
            Stocks = stockDataList
        };

        _cache.Set(cacheKey, finalResult, TimeSpan.FromMinutes(5));

        return Ok(finalResult);
    }

    public class SellRequest 
    { 
        public int PortfolioItemId { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public int Quantity { get; set; } 
    }

    [Authorize]
    [HttpPost("sell")]
    public async Task<IActionResult> SellStock([FromBody] SellRequest request)
    {
        try 
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            int userId = int.Parse(userIdClaim!);

            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == request.PortfolioItemId && p.UserId == userId);

            if (item == null) return NotFound("Акция не найдена в вашем портфеле");
            if (item.Quantity < request.Quantity) return BadRequest("Недостаточно акций для продажи");

            var priceData = await _stockService.GetStockPriceAsync(item.Ticker);
            if (priceData == null) return NotFound("Не удалось получить рыночную цену");

            decimal totalSale = priceData.CurrentPrice * request.Quantity;
            decimal commission = 1.0m;
            decimal finalAmount = totalSale - commission;

            var user = await _context.Users.FindAsync(userId);
            user!.Balance += finalAmount;

            if (item.Quantity == request.Quantity) {
                _context.PortfolioItems.Remove(item);
            } else {
                item.Quantity -= request.Quantity;
            }

            _context.Transactions.Add(new Transaction {
                UserId = userId,
                Type = "Sell",
                Ticker = item.Ticker,
                Amount = finalAmount,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Продано", received = finalAmount, commission });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [Authorize]
    [HttpPost("sell-grouped")]
    public async Task<IActionResult> SellGrouped([FromBody] SellRequest request)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        int userId = int.Parse(userIdClaim!);

        var items = await _context.PortfolioItems
            .Where(p => p.Ticker == request.Ticker.ToUpper() && p.UserId == userId)
            .OrderBy(p => p.PurchaseDate)
            .ToListAsync();

        int totalAvailable = items.Sum(i => i.Quantity);
        if (totalAvailable < request.Quantity) return BadRequest("Недостаточно акций");

        var priceData = await _stockService.GetStockPriceAsync(request.Ticker);
        if (priceData == null) return NotFound("Цена не найдена");

        int remainingToSell = request.Quantity;
        foreach (var item in items)
        {
            if (remainingToSell <= 0) break;

            if (item.Quantity <= remainingToSell)
            {
                remainingToSell -= item.Quantity;
                _context.PortfolioItems.Remove(item);
            }
            else
            {
                item.Quantity -= remainingToSell;
                remainingToSell = 0;
            }
        }

        decimal totalSale = priceData.CurrentPrice * request.Quantity;
        decimal finalAmount = totalSale - 1.0m;

        var user = await _context.Users.FindAsync(userId);
        user!.Balance += finalAmount;

        _context.Transactions.Add(new Transaction {
            UserId = userId,
            Type = "Sell",
            Ticker = request.Ticker.ToUpper(),
            Amount = finalAmount,
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();
        return Ok(new { newBalance = user.Balance });
    }

}
