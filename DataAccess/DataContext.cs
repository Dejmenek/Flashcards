using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess;

public class DataContext
{
    private readonly string _connectionString;

    public DataContext(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("LearnifyDb")!;
    }

    public void CreateDatabase()
    {
        CreateTables();
        SeedStacks();
        SeedFlashcards();
    }
    private void CreateTables()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.CreateTables;

            connection.Execute(sql);
        }
    }

    private void SeedStacks()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.SeedStacks;

            connection.Execute(sql);
        }
    }

    private void SeedFlashcards()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.SeedFlashcards;

            connection.Execute(sql);
        }
    }
}
