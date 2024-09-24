using Dapper;
using System.Configuration;
using System.Data.SqlClient;

namespace Flashcards.Dejmenek.DataAccess;

public static class DataContext
{
    private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnection"].ConnectionString;

    public static void CreateDatabase()
    {
        CreateTables();
        SeedStacks();
        SeedFlashcards();
    }
    private static void CreateTables()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.CreateTables;

            connection.Execute(sql);
        }
    }

    private static void SeedStacks()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.SeedStacks;

            connection.Execute(sql);
        }
    }

    private static void SeedFlashcards()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.SeedFlashcards;

            connection.Execute(sql);
        }
    }
}
