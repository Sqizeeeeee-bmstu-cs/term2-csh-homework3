using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using homework3.Configs;
using homework3.Services;
using homework3.Data;

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

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddMemoryCache();

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

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
