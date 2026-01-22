using Auth_Redis.Entities;
using Auth_Redis.Repositories.Interfaces;
using Auth_Redis.Services.Interfaces;

namespace Auth_Redis.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }
    }
}
