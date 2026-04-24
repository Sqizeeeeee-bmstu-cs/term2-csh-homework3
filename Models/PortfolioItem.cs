namespace homework3.Models;

public class PortfolioItem
{
    public int Id { get; set; }

    public string Ticker { get; set; } = string.Empty;
    
    public int Quantity { get; set; }

    public decimal BuyPrice { get; set; }

    public int UserId { get; set; }
    
    public User? User { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.Now;
}
