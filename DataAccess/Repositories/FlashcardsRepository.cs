using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class FlashcardsRepository : IFlashcardsRepository
{
    private readonly string _connectionString;

    public FlashcardsRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("LearnifyDb")!;
    }

    public void AddFlashcard(int stackId, string front, string back)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.AddFlashcard;

            connection.Execute(sql, new
            {
                StackId = stackId,
                Front = front,
                Back = back
            });
        }
    }

    public void DeleteFlashcard(int flashcardId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.DeleteFlashcard;

            connection.Execute(sql, new
            {
                Id = flashcardId
            });
        }
    }

    public IEnumerable<Flashcard> GetAllFlashcards()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetFlashcards;

            return connection.Query<Flashcard>(sql);
        }
    }

    public void UpdateFlashcard(int flashcardId, string front, string back)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.UpdateFlashcard;

            connection.Execute(sql, new
            {
                Front = front,
                Back = back,
                Id = flashcardId
            });
        }
    }
}
