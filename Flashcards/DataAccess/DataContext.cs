using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess;

public class DataContext
{
    private readonly string _connectionString;

    public DataContext(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")!;
    }

    public async Task CreateDatabase()
    {
        await CreateTables();
        await SeedStacks();
        await SeedFlashcards();
    }
    private async Task CreateTables()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.CreateTables;

            await connection.ExecuteAsync(sql);
        }
    }

    private async Task SeedStacks()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.SeedStacks;

            await connection.ExecuteAsync(sql);
        }
    }

    private async Task SeedFlashcards()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.SeedFlashcards;

            await connection.ExecuteAsync(sql);
        }
    }
}
