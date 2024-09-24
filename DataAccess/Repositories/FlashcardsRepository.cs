using Dapper;
using Flashcards.Dejmenek.DataAccess.Interfaces;
using Flashcards.Dejmenek.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace Flashcards.Dejmenek.DataAccess.Repositories;

public class FlashcardsRepository : IFlashcardsRepository
{
    private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnection"].ConnectionString;

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
