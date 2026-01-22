using Auth_Redis.Entities;

namespace Auth_Redis.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<Users>> GetAllUsersAsync();
    }
}
