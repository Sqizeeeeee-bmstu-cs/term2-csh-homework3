namespace homework3.Models;

public class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }
    
    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Ticker { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User User { get; set; }
}
