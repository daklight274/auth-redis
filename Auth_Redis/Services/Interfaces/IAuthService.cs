using Auth_Redis.DTOs.Requests;
using Auth_Redis.DTOs.Respones;

namespace Auth_Redis.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenRespone> LoginAsync(LoginRequest request);
        Task<string> RegisterAsync(RegisterRequest request);
        Task<TokenRespone> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<bool> VerifyEmailAsync(string username, string code);
    }
}
