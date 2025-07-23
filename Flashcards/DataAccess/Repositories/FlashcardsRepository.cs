using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;
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

    public async Task<Result> AddFlashcardAsync(int stackId, string front, string back)
    {
        try
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
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(FlashcardsErrors.AddFailed);
        }
        catch (Exception)
        {
            return Result.Failure(FlashcardsErrors.AddFailed);
        }
    }

    public async Task<Result> DeleteFlashcardAsync(int flashcardId)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.DeleteFlashcard;

                await connection.ExecuteAsync(sql, new
                {
                    Id = flashcardId
                });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(FlashcardsErrors.DeleteFailed);
        }
        catch (Exception)
        {
            return Result.Failure(FlashcardsErrors.DeleteFailed);
        }
    }

    public async Task<Result<IEnumerable<Flashcard>>> GetAllFlashcardsAsync()
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetFlashcards;

                var flashcards = await connection.QueryAsync<Flashcard>(sql);
                return Result.Success(flashcards);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<Flashcard>>(FlashcardsErrors.GetAllFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<Flashcard>>(FlashcardsErrors.GetAllFailed);
        }
    }

    public async Task<Result> UpdateFlashcardAsync(int flashcardId, string front, string back)
    {
        try
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
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(FlashcardsErrors.UpdateFailed);
        }
        catch (Exception)
        {
            return Result.Failure(FlashcardsErrors.UpdateFailed);
        }
    }
}
