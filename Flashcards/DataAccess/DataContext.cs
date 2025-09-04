using Dapper;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Flashcards.DataAccess;

public class DataContext
{
    private readonly string _defaultConnectionString;
    private readonly string _masterConnectionString;

    public DataContext(IConfiguration config)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
        _masterConnectionString = config.GetConnectionString("Master")!;
    }

    public async Task Init()
    {
        await CreateDatabase();
        await CreateTables();
        await CreateTypes();
        await CreateProcedures();
        await SeedStacks();
        await SeedFlashcards();
    }

    private async Task CreateDatabase()
    {
        using (var connection = new SqlConnection(_masterConnectionString))
        {
            string sql = SqlScripts.CreateDatabase;
            await connection.ExecuteAsync(sql);
        }
    }

    private async Task CreateTables()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.CreateTables;

            await connection.ExecuteAsync(sql);
        }
    }

    private async Task SeedStacks()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.SeedStacks;

            await connection.ExecuteAsync(sql);
        }
    }

    private async Task SeedFlashcards()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.SeedCards;

            await connection.ExecuteAsync(sql);
        }
    }

    private async Task CreateTypes()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.CreateTypes;
            await connection.ExecuteAsync(sql);
        }
    }

    private async Task CreateProcedures()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.CreateStoredProcedures;
            await connection.ExecuteAsync(sql);
        }
    }
}