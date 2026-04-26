

using Microsoft.EntityFrameworkCore;
using homework3.Models;

namespace homework3.Data;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PortfolioItem> PortfolioItems { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Transaction> Transactions { get; set; }
}
