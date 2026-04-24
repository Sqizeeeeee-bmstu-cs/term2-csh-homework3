using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using homework3.Data;
using homework3.Models;
using homework3.Exceptions;
using homework3.Configs;

namespace homework3.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ICustomLogger _logger;
    private readonly ApiConfig _config;

    public AuthService(AppDbContext context, ICustomLogger logger, ApiConfig config)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
    {
        if (!password.Any(char.IsUpper)) throw new ValidationException("no upper char");
        if (!password.Any(char.IsDigit)) throw new ValidationException("no number");
        if (password.Length < 8) throw new ValidationException("length");

        bool alreadyUsedEmail = await _context.Users.AnyAsync(u => u.Email == email);
        if (alreadyUsedEmail) throw new ValidationException("used email");

        string hashed_password = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashed_password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogAction($"Пользователь создан: {user.Username}", LogLevels.Info);

        var token = GenerateJwtToken(user);
        
        return new AuthResponse 
        {
            Id = user.Id,
            Username = user.Username,
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            _logger.LogAction($"Попытка входа: {email} не найден", LogLevels.Warning);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogAction($"Попытка входа: {email} неверный пароль", LogLevels.Warning);
            return null;
        }

        _logger.LogAction($"Пользователь {user.Username} вошел", LogLevels.Info);

        // Выдаем "паспорт" (токен) при успешном входе
        var token = GenerateJwtToken(user);

        return new AuthResponse 
        {
            Id = user.Id,
            Username = user.Username,
            Token = token
        };
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config.JwtSecret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
