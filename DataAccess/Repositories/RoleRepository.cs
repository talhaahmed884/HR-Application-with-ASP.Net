using Dapper;
using HR_Application.Models.Entities;

namespace HR_Application.DataAccess.Repositories;

/// <summary>
/// Implementation of IRoleRepository using Dapper for data access
/// Handles all role-related database operations
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of RoleRepository
    /// </summary>
    /// <param name="connectionFactory">Factory for creating database connections</param>
    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Retrieves a role by its unique ID
    /// </summary>
    public async Task<Role?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, RoleName, Description, CreatedAt, UpdatedAt
            FROM dbo.Roles
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Id = id });
    }

    /// <summary>
    /// Retrieves a role by its name
    /// </summary>
    public async Task<Role?> GetByNameAsync(string roleName)
    {
        const string sql = @"
            SELECT Id, RoleName, Description, CreatedAt, UpdatedAt
            FROM dbo.Roles
            WHERE RoleName = @RoleName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleName = roleName });
    }

    /// <summary>
    /// Retrieves all roles in the system
    /// </summary>
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, RoleName, Description, CreatedAt, UpdatedAt
            FROM dbo.Roles
            ORDER BY RoleName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Role>(sql);
    }
}
