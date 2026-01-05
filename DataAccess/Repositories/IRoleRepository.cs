using HR_Application.Models.Entities;

namespace HR_Application.DataAccess.Repositories;

/// <summary>
/// Repository interface for Role data access operations
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Retrieves a role by its unique ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role entity if found, null otherwise</returns>
    Task<Role?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a role by its name
    /// </summary>
    /// <param name="roleName">Name of the role</param>
    /// <returns>Role entity if found, null otherwise</returns>
    Task<Role?> GetByNameAsync(string roleName);

    /// <summary>
    /// Retrieves all roles in the system
    /// </summary>
    /// <returns>List of all roles</returns>
    Task<IEnumerable<Role>> GetAllAsync();
}
