using StackExchange.Redis;

namespace Shared.Utils.Redis.Services;

public class RedisService
{
    private readonly ConnectionMultiplexer _connection;
    
    
    public RedisService(string host, string password)
    {
        _connection = ConnectionMultiplexer.Connect(host);
        var database = _connection.GetDatabase();
        if (password.Length != 0)
            database.Execute("AUTH", password);
    }
    
    public ConnectionMultiplexer Connection => _connection;
}