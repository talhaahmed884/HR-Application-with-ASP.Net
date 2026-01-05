using HR_Application.Models.Entities;

namespace HR_Application.DataAccess.Repositories;

public interface IUserRepository
{
    Task<Employee?> GetByIdAsync(int id);

    Task<Employee?> GetByEmailAsync(string email);

    Task<IEnumerable<Employee>> GetAllAsync();

    Task<IEnumerable<Employee>> GetByRoleIdAsync(int roleId);

    Task<Employee> CreateAsync(Employee employee);

    Task<bool> UpdateAsync(Employee employee);

    Task<bool> DeleteAsync(int id);

    Task<bool> EmailExistsAsync(string email);

    Task<Dictionary<int, int>> GetEmployeeCountByRoleAsync();
}
