

using homework3.Models;

namespace homework3.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(string username, string email, string password);

    Task<AuthResponse> LoginAsync(string email, string password);
}
