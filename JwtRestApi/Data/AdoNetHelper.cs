using Dapper;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AdoNetHelper
{
    private readonly string _connString;
    public AdoNetHelper(IConfiguration config) => _connString = config.GetConnectionString("DefaultConnection");

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var conn = new SQLiteConnection(_connString);
        return await conn.QueryAsync<T>(sql, param);
    }
}
