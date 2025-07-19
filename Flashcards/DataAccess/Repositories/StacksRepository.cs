using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class StacksRepository : IStacksRepository
{
    private readonly string _connectionString;

    public StacksRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")!;
    }

    public async Task AddStackAsync(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.AddStack;

            await connection.ExecuteAsync(sql, new
            {
                Name = name
            });
        }
    }

    public async Task DeleteFlashcardFromStackAsync(int flashcardId, int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.DeleteFlashcardFromStack;

            await connection.ExecuteAsync(sql, new
            {
                Id = flashcardId,
                StackId = stackId
            });
        }
    }

    public async Task<IEnumerable<Flashcard>> GetFlashcardsByStackIdAsync(int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetFlashcardsByStackId;

            return await connection.QueryAsync<Flashcard>(sql, new
            {
                StackId = stackId
            });
        }
    }

    public async Task UpdateFlashcardInStackAsync(int flashcardId, int stackId, string front, string back)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.UpdateFlashcardInStack;

            await connection.ExecuteAsync(sql, new
            {
                Front = front,
                Back = back,
                Id = flashcardId,
                StackId = stackId
            });
        }
    }

    public async Task DeleteStackAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.DeleteStack;

            await connection.ExecuteAsync(sql, new
            {
                Id = id
            });
        }
    }

    public async Task<IEnumerable<Stack>> GetAllStacksAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetStacks;

            return await connection.QueryAsync<Stack>(sql);
        }
    }

    public async Task<Stack> GetStackAsync(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetStack;

            return await connection.QuerySingleAsync<Stack>(sql, new
            {
                Name = name
            });
        }
    }

    public async Task<bool> StackExistsWithNameAsync(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.StackExistsWithName;

            return await connection.QuerySingleAsync<bool>(sql, new
            {
                Name = name
            });
        }
    }

    public async Task<bool> HasStackAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.HasStack;

            return await connection.QuerySingleAsync<bool>(sql);
        }
    }

    public async Task<bool> HasStackAnyFlashcardsAsync(int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.HasStackAnyFlashcards;

            return await connection.QuerySingleAsync<bool>(sql, new
            {
                Id = stackId
            });
        }
    }

    public async Task<int> GetFlashcardsCountInStackAsync(int stackId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetFlashcardsCountInStack;

            return await connection.QuerySingleAsync<int>(sql, new
            {
                StackId = stackId
            });
        }
    }
}
