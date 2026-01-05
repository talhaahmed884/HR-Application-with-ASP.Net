using Dapper;
using HR_Application.Models.Entities;

namespace HR_Application.DataAccess.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuthRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserPassword?> GetUserPasswordAsync(int userId)
    {
        const string sql = @"
            SELECT
                UserId, PasswordHash, Salt, CreatedAt, UpdatedAt
            FROM dbo.UserPasswords
            WHERE UserId = @UserId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<UserPassword>(sql, new { UserId = userId });
    }

    public async Task<bool> CreateUserPasswordAsync(UserPassword userPassword)
    {
        const string sql = @"
            INSERT INTO dbo.UserPasswords
                (UserId, PasswordHash, Salt, CreatedAt, UpdatedAt)
            VALUES
                (@UserId, @PasswordHash, @Salt, GETUTCDATE(), GETUTCDATE())";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, userPassword);

        return affectedRows > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        const string sql = @"
            UPDATE dbo.UserPasswords
            SET
                PasswordHash = @PasswordHash,
                UpdatedAt = GETUTCDATE()
            WHERE UserId = @UserId";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = newPasswordHash });

        return affectedRows > 0;
    }
}
