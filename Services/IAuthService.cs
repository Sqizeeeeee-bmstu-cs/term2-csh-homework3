

using homework3.Models;

namespace homework3.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(string username, string email, string password);

    Task<User?> LoginAsync(string email, string password);
}
