using HR_Application.Models.Entities;

namespace HR_Application.DataAccess.Repositories;

public interface IAuthRepository
{
    Task<UserPassword?> GetUserPasswordAsync(int userId);
    Task<bool> CreateUserPasswordAsync(UserPassword userPassword);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
}
