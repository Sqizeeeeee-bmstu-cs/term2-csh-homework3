using homework3.Models;
using Microsoft.EntityFrameworkCore;

namespace homework3.Data;

/// <summary>
/// Контекст базы данных для приложения
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// DbSet для таблицы Departments
    /// </summary>
    public DbSet<Department> Departments { get; set; }

    /// <summary>
    /// DbSet для таблицы Professors
    /// </summary>
    public DbSet<Professor> Professors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=app.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Связь один-ко-многим
        modelBuilder.Entity<Professor>()
            .HasOne(p => p.Department)
            .WithMany(d => d.Professors)
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
