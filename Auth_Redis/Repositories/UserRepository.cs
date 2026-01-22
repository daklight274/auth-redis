using Auth_Redis.Entities;
using Auth_Redis.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Auth_Redis.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<int> InsertAsync(Users user)
        {
            try
            {
                int rowsAffected = 0;
                using (SqlConnection con = new(_connectionString))
                {
                    if(con.State == System.Data.ConnectionState.Closed)
                    {
                        await con.OpenAsync();
                    }
                    DynamicParameters param = new();
                    param.Add("@Id", user.Id);
                    param.Add("@UserName", user.UserName);
                    param.Add("@Email", user.Email);
                    param.Add("@Password", user.Password);
                    param.Add("@IsEmailConfirmed", user.IsEmailConfirmed);

                    rowsAffected = await con.ExecuteAsync("spUser_Insert", param, commandType: System.Data.CommandType.StoredProcedure);
                }
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
        public async Task<int> ChangeEmailComfirmed(string username, bool isEmailComfirmed)
        {
            try
            {
                int rowsAffected = 0;
                using (SqlConnection con = new(_connectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        await con.OpenAsync();
                    }
                    DynamicParameters param = new();
                    param.Add("@UserName", username);
                    param.Add("@IsEmailConfirmed", isEmailComfirmed);
                    rowsAffected = await con.ExecuteAsync("spUser_ChangeEmailConfirmed", param, commandType: System.Data.CommandType.StoredProcedure);
                }
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public async Task<Users> GetAsync(string username)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    await con.OpenAsync();
                }

                DynamicParameters param = new();
                param.Add("@UserName", username);

                var user = await con.QueryFirstOrDefaultAsync<Users>("spUser_GetByUsername", param, commandType: System.Data.CommandType.StoredProcedure);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> CheckExistUser(string username)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    await con.OpenAsync();
                }

                var sql = @"SELECT IIF(EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName), 1, 0)";
                var result = await con.ExecuteScalarAsync<bool>(sql, new { UserName = username });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public async Task<List<Users>> GetAllUsersAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    await con.OpenAsync();
                }
                var sql = "SELECT * FROM Users";
                var users = await con.QueryAsync<Users>(sql);
                return users.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return [];
            }
        }
    }
}
