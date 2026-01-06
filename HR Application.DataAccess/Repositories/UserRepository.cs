using Dapper;
using HR_Application.Model.Entities;

namespace HR_Application.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT
                e.Id, e.Email, e.Name, e.Address, e.CellNumber,
                e.RoleId, r.RoleName, e.IsActive, e.CreatedAt, e.UpdatedAt
            FROM dbo.Employees e
            INNER JOIN dbo.Roles r ON e.RoleId = r.Id
            WHERE e.Id = @Id";

        // Using 'using' statement ensures proper disposal of connection
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT
                e.Id, e.Email, e.Name, e.Address, e.CellNumber,
                e.RoleId, r.RoleName, e.IsActive, e.CreatedAt, e.UpdatedAt
            FROM dbo.Employees e
            INNER JOIN dbo.Roles r ON e.RoleId = r.Id
            WHERE e.Email = @Email";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Email = email });
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        const string sql = @"
            SELECT
                e.Id, e.Email, e.Name, e.Address, e.CellNumber,
                e.RoleId, r.RoleName, e.IsActive, e.CreatedAt, e.UpdatedAt
            FROM dbo.Employees e
            INNER JOIN dbo.Roles r ON e.RoleId = r.Id
            ORDER BY e.Name";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Employee>(sql);
    }

    public async Task<IEnumerable<Employee>> GetByRoleIdAsync(int roleId)
    {
        const string sql = @"
            SELECT
                e.Id, e.Email, e.Name, e.Address, e.CellNumber,
                e.RoleId, r.RoleName, e.IsActive, e.CreatedAt, e.UpdatedAt
            FROM dbo.Employees e
            INNER JOIN dbo.Roles r ON e.RoleId = r.Id
            WHERE e.RoleId = @RoleId
            ORDER BY e.Name";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Employee>(sql, new { RoleId = roleId });
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        const string sql = @"
            INSERT INTO dbo.Employees (Email, Name, Address, CellNumber, RoleId, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Email, @Name, @Address, @CellNumber, @RoleId, @IsActive, GETUTCDATE(), GETUTCDATE());

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();

        // Execute the insert and get the new ID
        var newId = await connection.ExecuteScalarAsync<int>(sql, employee);

        // Retrieve and return the newly created employee
        return await GetByIdAsync(newId) ?? employee;
    }

    public async Task<bool> UpdateAsync(Employee employee)
    {
        const string sql = @"
            UPDATE dbo.Employees
            SET
                Name = @Name,
                Address = @Address,
                CellNumber = @CellNumber,
                RoleId = @RoleId,
                IsActive = @IsActive,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, employee);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dbo.Employees WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });

        return affectedRows > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Employees WHERE Email = @Email";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });

        return count > 0;
    }

    public async Task<Dictionary<int, int>> GetEmployeeCountByRoleAsync()
    {
        const string sql = @"
            SELECT
                RoleId,
                COUNT(*) AS EmployeeCount
            FROM dbo.Employees
            WHERE IsActive = 1
            GROUP BY RoleId";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<(int RoleId, int EmployeeCount)>(sql);

        return results.ToDictionary(x => x.RoleId, x => x.EmployeeCount);
    }
}
