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
        private readonly IEmailService _emailService;
        public AuthService(IRedisService redisService, IUserRepository userRepository, IEmailService emailService)
        {
            _redisService = redisService;
            _userRepository = userRepository;
            _emailService = emailService;
        }
        public async Task<TokenRespone> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetAsync(request.UserName);
            if (user == null) throw new Exception("User not found");

            if(!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new Exception("Password is incorrect");
            }

            if(user.IsEmailConfirmed == false)
            {
                // Tạo code 6 chữ số
                string code = new Random().Next(100000, 999999).ToString();

                // Lưu vào Redis 2 phút
                await _redisService.SetAsync($"email_verif:{user.UserName}", code, TimeSpan.FromMinutes(2));

                // Gửi code
                await _emailService.SendEmailAsync(user.Email, "Mã xác thực email", $"Mã của bạn là: {code}");
                throw new Exception("Email not confirmed. Please check your email.");
            }

            var accessToken = JwtHelper.GenerateToken(user.Id, 1); // 1 phút
            var refreshToken = Guid.NewGuid().ToString();

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

            // Tạo code 6 chữ số
            string code = new Random().Next(100000, 999999).ToString();

            // Lưu vào Redis 2 phút
            await _redisService.SetAsync($"email_verif:{user.UserName}", code, TimeSpan.FromMinutes(2));

            // Gửi code
            await _emailService.SendEmailAsync(user.Email, "Mã xác thực email", $"Mã của bạn là: {code}");
            return "Register successful. Please check your email.";
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            // Xóa token khỏi Redis
            await _redisService.DeleteAsync($"refresh:{refreshToken}");
        }

        // Verify code
        public async Task<bool> VerifyEmailAsync(string username, string code)
        {
            var storedCode = await _redisService.GetAsync($"email_verif:{username}");
            if (storedCode == code)
            {
                await _userRepository.ChangeEmailComfirmed(username, true);
                await _redisService.DeleteAsync($"email_verif:{username}");
                return true;
            }
            return false;
        }
    }
}
