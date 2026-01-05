using System.Data;

namespace HR_Application.DataAccess;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
