using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class StacksRepository : IStacksRepository
{
    private readonly string _defaultConnectionString;

    public StacksRepository(IConfiguration config)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
    }

    public async Task<Result> AddStackAsync(string name)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddStack;

                await connection.ExecuteAsync(sql, new { Name = name });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(StacksErrors.AddFailed);
        }
        catch (Exception)
        {
            return Result.Failure(StacksErrors.AddFailed);
        }
    }

    public async Task<Result> DeleteFlashcardFromStackAsync(int flashcardId, int stackId)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.DeleteFlashcardFromStack;

                await connection.ExecuteAsync(sql, new { Id = flashcardId, StackId = stackId });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(StacksErrors.DeleteFlashcardFailed);
        }
        catch (Exception)
        {
            return Result.Failure(StacksErrors.DeleteFlashcardFailed);
        }
    }

    public async Task<Result<IEnumerable<Flashcard>>> GetFlashcardsByStackIdAsync(int stackId)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetFlashcardsByStackId;

                var flashcards = await connection.QueryAsync<Flashcard>(sql, new { StackId = stackId });
                return Result.Success(flashcards);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<Flashcard>>(StacksErrors.GetFlashcardsByStackIdFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<Flashcard>>(StacksErrors.GetFlashcardsByStackIdFailed);
        }
    }

    public async Task<Result> UpdateFlashcardInStackAsync(int flashcardId, int stackId, string front, string back)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateFlashcardInStack;

                await connection.ExecuteAsync(sql, new { Front = front, Back = back, Id = flashcardId, StackId = stackId });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(StacksErrors.UpdateFailed);
        }
        catch (Exception)
        {
            return Result.Failure(StacksErrors.UpdateFailed);
        }
    }

    public async Task<Result> DeleteStackAsync(int id)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.DeleteStack;

                await connection.ExecuteAsync(sql, new { Id = id });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(StacksErrors.DeleteStackFailed);
        }
        catch (Exception)
        {
            return Result.Failure(StacksErrors.DeleteStackFailed);
        }
    }

    public async Task<Result<IEnumerable<Stack>>> GetAllStacksAsync()
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetStacks;

                var stacks = await connection.QueryAsync<Stack>(sql);
                return Result.Success(stacks);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed);
        }
    }

    public async Task<Result<Stack>> GetStackAsync(string name)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetStack;

                var stack = await connection.QuerySingleAsync<Stack>(sql, new { Name = name });
                return Result.Success(stack);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<Stack>(StacksErrors.GetStackFailed);
        }
        catch (Exception)
        {
            return Result.Failure<Stack>(StacksErrors.GetStackFailed);
        }
    }

    public async Task<Result<bool>> StackExistsWithNameAsync(string name)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.StackExistsWithName;

                var exists = await connection.QuerySingleAsync<bool>(sql, new { Name = name });
                return Result.Success(exists);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<bool>(StacksErrors.StackWithNameFailed);
        }
        catch (Exception)
        {
            return Result.Failure<bool>(StacksErrors.StackWithNameFailed);
        }
    }

    public async Task<Result<bool>> HasStackAnyFlashcardsAsync(int stackId)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.HasStackAnyFlashcards;

                var hasAny = await connection.QuerySingleAsync<bool>(sql, new { Id = stackId });
                return Result.Success(hasAny);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<bool>(StacksErrors.HasAnyFlashcardsFailed);
        }
        catch (Exception)
        {
            return Result.Failure<bool>(StacksErrors.HasAnyFlashcardsFailed);
        }
    }

    public async Task<Result<int>> GetFlashcardsCountInStackAsync(int stackId)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetFlashcardsCountInStack;

                var count = await connection.QuerySingleAsync<int>(sql, new { StackId = stackId });
                return Result.Success(count);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<int>(StacksErrors.GetFlashcardsCountFailed);
        }
        catch (Exception)
        {
            return Result.Failure<int>(StacksErrors.GetFlashcardsCountFailed);
        }
    }
}
