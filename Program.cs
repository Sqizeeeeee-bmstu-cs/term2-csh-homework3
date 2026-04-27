using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using homework3.Configs;
using homework3.Services;
using homework3.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICustomLogger, ConsoleLogger>();


var apiConfig = builder.Configuration.GetSection("FinnhubApi").Get<ApiConfig>();

if (apiConfig == null || string.IsNullOrEmpty(apiConfig.ApiKey) || string.IsNullOrEmpty(apiConfig.JwtSecret))
{
    throw new Exception(
        "Критическая ошибка: не найдены FinnhubApi.ApiKey или FinnhubApi.JwtSecret в конфигурации (appsettings.* или переменные окружения)."
    );
}

builder.Services.AddSingleton(apiConfig);


builder.Services.AddHttpClient<StockService>();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite("Data Source=/app/data/stockhub.db"));

builder.Services.AddScoped<IAuthService, AuthService>();

var key = Encoding.ASCII.GetBytes(apiConfig.JwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseDefaultFiles();
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
