using Auth_Redis.DTOs.Requests;
using Auth_Redis.DTOs.Respones;
using Auth_Redis.Entities;
using Auth_Redis.Helpers;
using Auth_Redis.Repositories.Interfaces;
using Auth_Redis.Services.Interfaces;
using BCrypt.Net;
using StackExchange.Redis;

namespace Auth_Redis.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRedisService _redisService;
        private readonly IUserRepository _userRepository;
        public AuthService(IRedisService redisService, IUserRepository userRepository)
        {
            _redisService = redisService;
            _userRepository = userRepository;
        }
        public async Task<TokenRespone> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetAsync(request.UserName);
            if (user == null) throw new Exception("User not found");

            if(!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new Exception("Password is incorrect");
            }

            var accessToken = JwtHelper.GenerateToken(user.Id, 1); // 1 phút
            var refreshToken = Guid.NewGuid().ToString();

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            Console.WriteLine($"ValidFrom: {jwt.ValidFrom.ToLocalTime()}, ValidTo: {jwt.ValidTo.ToLocalTime()}");

            // Lưu refresh token vào Redis 7 ngày
            await _redisService.SetAsync($"refresh:{refreshToken}", user.Id, TimeSpan.FromDays(7));

            return new TokenRespone { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<TokenRespone> RefreshTokenAsync(string refreshToken)
        {
            var userId = await _redisService.GetAsync($"refresh:{refreshToken}");
            if (userId == null) throw new Exception("User Invalid");

            var accessToken = JwtHelper.GenerateToken(userId, 1);
            var newRefreshToken = Guid.NewGuid().ToString();

            // Xóa refresh cũ, lưu refresh mới
            await _redisService.DeleteAsync($"refresh:{refreshToken}");
            await _redisService.SetAsync($"refresh:{newRefreshToken}", userId, TimeSpan.FromDays(7));

            return new TokenRespone { AccessToken = accessToken, RefreshToken = newRefreshToken };
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            var isUserExist = await _userRepository.GetAsync(request.UserName);
            if (isUserExist != null) throw new Exception("User already exists");

            var user = new Users
            {
                Id = Guid.NewGuid().ToString(),
                UserName = request.UserName,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password), // Lưu ý: Cần mã hóa mật khẩu trong thực tế
                IsEmailConfirmed = false
            };

            var result = await _userRepository.InsertAsync(user);
            if (result <= 0) throw new Exception("Register failed");
            return "Register successful";
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            // Xóa token khỏi Redis
            await _redisService.DeleteAsync($"refresh:{refreshToken}");
        }
    }
}
