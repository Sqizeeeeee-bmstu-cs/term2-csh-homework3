using Microsoft.Extensions.Configuration;
using homework3.Configs;
using homework3.Services;
using homework3.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddSingleton<ICustomLogger, ConsoleLogger>();

var apiConfig = builder.Configuration.GetSection("FinnhubApi").Get<ApiConfig>();
if (apiConfig != null) 
{
    builder.Services.AddSingleton(apiConfig);
}

builder.Services.AddHttpClient<StockService>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=stockhub.db")); 

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
