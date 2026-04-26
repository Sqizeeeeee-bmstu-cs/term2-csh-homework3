namespace homework3.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;

    public decimal Balance { get; set; } = 0m;

    public List<PortfolioItem> Portfolio { get; set; } = new();
}
