using Common.Utilities;

namespace Module.Auth.Application.Abstraction;

public interface IDbConnectionTester
{
    Task<Result<bool>> TestConnectionAsync(string databaseName, string? schema = null);
}