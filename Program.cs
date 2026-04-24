using Microsoft.Extensions.Configuration;
using homework3.Configs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


var apiConfig = builder.Configuration.GetSection("FinnhubApi").Get<ApiConfig>();

if (apiConfig != null) 
{
    builder.Services.AddSingleton(apiConfig);
}


var app = builder.Build();

builder.Services.AddHttpClient<StockService>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();