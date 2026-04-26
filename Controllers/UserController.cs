using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


using homework3.Models;
using homework3.Data;
using homework3.Services;

namespace homework3.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICustomLogger _logger;

    public UserController(AppDbContext context, ICustomLogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public class TopUpRequest { public decimal Amount { get; set; } }

    [Authorize]
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
    {

        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null) return Unauthorized();
        int userId = int.Parse(userIdClaim);

        if (request.Amount <= 0 || request.Amount > 10000)
        {
            return BadRequest("Сумма пополнения должна быть от 1 до 10 000$");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("Пользователь не найден");

        user.Balance += request.Amount;

        var transaction = new Transaction
        {
            UserId = userId,
            Type = "TopUp",
            Amount = request.Amount,
            CreatedAt = DateTime.Now
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        _logger.LogAction($"Пользователь {user.Username} пополнил баланс на {request.Amount}$", LogLevels.Info);

        return Ok(new { message = "Баланс успешно пополнен", newBalance = user.Balance });
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

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null) return Unauthorized();
        int userId = int.Parse(userIdClaim);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        int sellCount = await _context.Transactions
            .CountAsync(t => t.UserId == userId && t.Type == "Sell");

        decimal totalCommissionPaid = sellCount * 1.0m;

        return Ok(new { 
            username = user.Username, 
            email = user.Email,
            balance = user.Balance,
            totalCommission = totalCommissionPaid
        });
    }


    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null) return Unauthorized();
        int userId = int.Parse(userIdClaim);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("portfolio-stats")]
    public async Task<IActionResult> GetPortfolioStats()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null) return Unauthorized();
        int userId = int.Parse(userIdClaim);

        var portfolio = await _context.PortfolioItems
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var stats = portfolio.GroupBy(p => p.Ticker)
            .Select(group => new
            {
                Ticker = group.Key,
                TotalQuantity = group.Sum(x => x.Quantity),

                AveragePrice = group.Sum(x => x.BuyPrice * x.Quantity) / group.Sum(x => x.Quantity),

                History = group.OrderBy(x => x.PurchaseDate).Select(x => new {
                    x.Id,
                    x.Quantity,
                    x.BuyPrice,
                    x.PurchaseDate
                })
            }).ToList();

        return Ok(stats);
    }

}
