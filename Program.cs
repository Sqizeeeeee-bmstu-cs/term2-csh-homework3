using homework3.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AppDbContext>();

var app = builder.Build();

// Create/migrate database and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    SeedDatabase(dbContext);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDatabase(AppDbContext context)
{
    // Only seed if data doesn't exist
    if (context.Departments.Any() || context.Professors.Any())
        return;

    var departments = new[]
    {
        new homework3.Models.Department { Name = "Информатика" },
        new homework3.Models.Department { Name = "Математика" },
        new homework3.Models.Department { Name = "Физика" },
        new homework3.Models.Department { Name = "Химия" }
    };

    context.Departments.AddRange(departments);
    context.SaveChanges();

    var professors = new[]
    {
        new homework3.Models.Professor { DepartmentId = 1, Name = "Иван Петров", Publications = 15 },
        new homework3.Models.Professor { DepartmentId = 1, Name = "Мария Сидорова", Publications = 22 },
        new homework3.Models.Professor { DepartmentId = 1, Name = "Алексей Васильев", Publications = 18 },
        new homework3.Models.Professor { DepartmentId = 2, Name = "Елена Смирнова", Publications = 25 },
        new homework3.Models.Professor { DepartmentId = 2, Name = "Николай Морозов", Publications = 12 },
        new homework3.Models.Professor { DepartmentId = 2, Name = "Анна Лебедева", Publications = 30 },
        new homework3.Models.Professor { DepartmentId = 3, Name = "Виктор Кузнецов", Publications = 20 },
        new homework3.Models.Professor { DepartmentId = 3, Name = "Татьяна Волкова", Publications = 17 },
        new homework3.Models.Professor { DepartmentId = 3, Name = "Дмитрий Соколов", Publications = 28 },
        new homework3.Models.Professor { DepartmentId = 4, Name = "Светлана Новикова", Publications = 14 },
        new homework3.Models.Professor { DepartmentId = 4, Name = "Константин Романов", Publications = 21 },
        new homework3.Models.Professor { DepartmentId = 4, Name = "Ольга Федорова", Publications = 19 }
    };

    context.Professors.AddRange(professors);
    context.SaveChanges();
}
