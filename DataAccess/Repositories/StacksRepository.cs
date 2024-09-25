using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class StacksRepository : IStacksRepository
{
    private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnection"].ConnectionString;
    public void AddStack(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.AddStack;

            connection.Execute(sql, new
            {
                Name = name
            });
        }
    }

    public void DeleteFlashcardFromStack(int flashcardId, int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.DeleteFlashcardFromStack;

            connection.Execute(sql, new
            {
                Id = flashcardId,
                StackId = stackId
            });
        }
    }

    public IEnumerable<Flashcard> GetFlashcardsByStackId(int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetFlashcardsByStackId;

            return connection.Query<Flashcard>(sql, new
            {
                StackId = stackId
            });
        }
    }

    public void UpdateFlashcardInStack(int flashcardId, int stackId, string front, string back)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.UpdateFlashcardInStack;

            connection.Execute(sql, new
            {
                Front = front,
                Back = back,
                Id = flashcardId,
                StackId = stackId
            });
        }
    }

    public void DeleteStack(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.DeleteStack;

            connection.Execute(sql, new
            {
                Id = id
            });
        }
    }

    public IEnumerable<Stack> GetAllStacks()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetStacks;

            return connection.Query<Stack>(sql);
        }
    }

    public Stack GetStack(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetStack;

            return connection.QuerySingle<Stack>(sql, new
            {
                Name = name
            });
        }
    }

    public bool StackExistsWithName(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.StackExistsWithName;

            return connection.QuerySingle<bool>(sql, new
            {
                Name = name
            });
        }
    }

    public bool HasStack()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.HasStack;

            return connection.QuerySingle<bool>(sql);
        }
    }

    public bool HasStackAnyFlashcards(int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.HasStackAnyFlashcards;

            return connection.QuerySingle<bool>(sql, new
            {
                Id = stackId
            });
        }
    }

    public int GetFlashcardsCountInStack(int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetFlashcardsCountInStack;

            return connection.QuerySingle<int>(sql, new
            {
                StackId = stackId
            });
        }
    }
}
