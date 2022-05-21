using System.Data;

namespace API.Data;

public interface IConnectionService
{
    public IDbConnection GetPostgresConnection();

    public void CloseConnection();
}
