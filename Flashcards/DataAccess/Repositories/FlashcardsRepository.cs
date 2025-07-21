using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class FlashcardsRepository : IFlashcardsRepository
{
    private readonly string _defaultConnectionString;

    public FlashcardsRepository(IConfiguration config)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
    }

    public async Task AddFlashcardAsync(int stackId, string front, string back)
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.AddFlashcard;

            await connection.ExecuteAsync(sql, new
            {
                StackId = stackId,
                Front = front,
                Back = back
            });
        }
    }

    public async Task DeleteFlashcardAsync(int flashcardId)
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.DeleteFlashcard;

            await connection.ExecuteAsync(sql, new
            {
                Id = flashcardId
            });
        }
    }

    public async Task<IEnumerable<Flashcard>> GetAllFlashcardsAsync()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.GetFlashcards;

            return await connection.QueryAsync<Flashcard>(sql);
        }
    }

    public async Task UpdateFlashcardAsync(int flashcardId, string front, string back)
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.UpdateFlashcard;

            await connection.ExecuteAsync(sql, new
            {
                Front = front,
                Back = back,
                Id = flashcardId
            });
        }
    }
}
