using Auth_Redis.Entities;

namespace Auth_Redis.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<int> InsertAsync(Users user);
        Task<Users> GetAsync(string username);
        Task<int> ChangeEmailComfirmed(string username, bool isEmailComfirmed);
        Task<bool> CheckExistUser(string username);
        Task<List<Users>> GetAllUsersAsync();
    }
}
