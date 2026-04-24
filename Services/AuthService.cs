using homework3.Data;
using homework3.Models;
using homework3.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace homework3.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ICustomLogger _logger;

    public AuthService(AppDbContext context, ICustomLogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(string username, string email, string password)
    {

        if (!password.Any(char.IsUpper)){
            throw new ValidationException("no upper char");
        }

        if (!password.Any(char.IsDigit))
        {
            throw new ValidationException("no number");
        }

        if (password.Length < 8)
        {
            throw new ValidationException("length");
        }

        bool alreadyUsedEmail = await _context.Users.AnyAsync(u => u.Email == email);

        if (alreadyUsedEmail == true)
        {
            throw new ValidationException("used email");
        }

        string hashed_password = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashed_password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogAction($"Пользователь создан, имя: {user.Username}, почта: {user.Email}",LogLevels.Info);


        return user;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            _logger.LogAction($"gопытка входа email: {email}, пользователь не найден", LogLevels.Warning);
            return null;
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!isPasswordValid)
        {
            _logger.LogAction($"попытка входа email: {email}, неверный пароль!", LogLevels.Warning);
            return null;
        }

        _logger.LogAction($"пользователь email: {email} успешно вошел в систему", LogLevels.Info);
        return user; 
    }
}
