using Dapper;

using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flashcards.DataAccess.Repositories;

public class StacksRepository : IStacksRepository
{
    private readonly string _defaultConnectionString;
    private readonly ILogger<StacksRepository> _logger;

    public StacksRepository(IConfiguration config, ILogger<StacksRepository> logger)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
        _logger = logger;
    }

    public async Task<Result> AddStackAsync(string name)
    {
        _logger.LogInformation("Adding stack with name {Name}.", name);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddStack;
                await connection.ExecuteAsync(sql, new { Name = name });
            }
            _logger.LogInformation("Successfully added stack {Name}.", name);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while adding stack {Name}.", name);
            return Result.Failure(StacksErrors.AddFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding stack {Name}.", name);
            return Result.Failure(StacksErrors.AddFailed);
        }
    }

    public async Task<Result> DeleteCardFromStackAsync(int cardId, int stackId)
    {
        _logger.LogInformation("Deleting card {CardId} from stack {StackId}.", cardId, stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.DeleteCardFromStack;
                await connection.ExecuteAsync(sql, new { Id = cardId, StackId = stackId });
            }
            _logger.LogInformation("Successfully deleted card {CardId} from stack {StackId}.", cardId, stackId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while deleting card {CardId} from stack {StackId}.", cardId, stackId);
            return Result.Failure(StacksErrors.DeleteCardFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting card {CardId} from stack {StackId}.", cardId, stackId);
            return Result.Failure(StacksErrors.DeleteCardFailed);
        }
    }

    public async Task<Result<IEnumerable<BaseCard>>> GetCardsByStackIdAsync(int stackId)
    {
        _logger.LogInformation("Retrieving cards for stack {StackId}.", stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetCardsByStackId;
                using (var reader = await connection.ExecuteReaderAsync(sql, new { StackId = stackId }))
                {
                    var flashcardParser = reader.GetRowParser<Flashcard>();
                    var clozeCardParser = reader.GetRowParser<ClozeCard>();
                    var fillInCardParser = reader.GetRowParser<FillInCard>();
                    var multipleChoiceCardParser = reader.GetRowParser<MultipleChoiceCard>();

                    var cards = new List<BaseCard>();
                    while (await reader.ReadAsync())
                    {
                        var discriminator = reader.GetString(reader.GetOrdinal("CardType"));

                        if (!Enum.TryParse(discriminator, out CardType cardType))
                        {
                            _logger.LogWarning("Unknown card type discriminator: {Discriminator}", discriminator);
                            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
                        }

                        BaseCard card = cardType switch
                        {
                            CardType.Flashcard => flashcardParser(reader),
                            CardType.Cloze => clozeCardParser(reader),
                            CardType.FillIn => fillInCardParser(reader),
                            CardType.MultipleChoice => multipleChoiceCardParser(reader),
                            _ => throw new InvalidOperationException("Unknown card type")
                        };
                        cards.Add(card);
                    }

                    _logger.LogInformation("Successfully retrieved {Count} cards for stack {StackId}.", cards.Count, stackId);
                    return Result.Success(cards.AsEnumerable());
                }
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving cards for stack {StackId}.", stackId);
            return Result.Failure<IEnumerable<BaseCard>>(StacksErrors.GetCardsByStackIdFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving cards for stack {StackId}.", stackId);
            return Result.Failure<IEnumerable<BaseCard>>(StacksErrors.GetCardsByStackIdFailed);
        }
    }

    public async Task<Result> UpdateFlashcardInStackAsync(int flashcardId, int stackId, string front, string back)
    {
        _logger.LogInformation("Updating flashcard {CardId} in stack {StackId}.", flashcardId, stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateFlashcardInStack;
                await connection.ExecuteAsync(sql, new { Front = front, Back = back, Id = flashcardId, StackId = stackId });
            }
            _logger.LogInformation("Successfully updated flashcard {CardId} in stack {StackId}.", flashcardId, stackId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating flashcard {CardId} in stack {StackId}.", flashcardId, stackId);
            return Result.Failure(StacksErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating flashcard {CardId} in stack {StackId}.", flashcardId, stackId);
            return Result.Failure(StacksErrors.UpdateFailed);
        }
    }

    public async Task<Result> DeleteStackAsync(int stackId)
    {
        _logger.LogInformation("Deleting stack {StackId}.", stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.DeleteStack;
                await connection.ExecuteAsync(sql, new { Id = stackId });
            }
            _logger.LogInformation("Successfully deleted stack {StackId}.", stackId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while deleting stack {StackId}.", stackId);
            return Result.Failure(StacksErrors.DeleteStackFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting stack {StackId}.", stackId);
            return Result.Failure(StacksErrors.DeleteStackFailed);
        }
    }

    public async Task<Result<IEnumerable<Stack>>> GetAllStacksAsync()
    {
        _logger.LogInformation("Retrieving all stacks.");
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetStacks;
                var stacks = await connection.QueryAsync<Stack>(sql);
                _logger.LogInformation("Successfully retrieved {Count} stacks.", stacks.Count());
                return Result.Success(stacks);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving all stacks.");
            return Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving all stacks.");
            return Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed);
        }
    }

    public async Task<Result<Stack>> GetStackAsync(string name)
    {
        _logger.LogInformation("Retrieving stack with name {Name}.", name);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetStack;
                var stack = await connection.QuerySingleAsync<Stack>(sql, new { Name = name });
                _logger.LogInformation("Successfully retrieved stack {Name}.", name);
                return Result.Success(stack);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving stack {Name}.", name);
            return Result.Failure<Stack>(StacksErrors.GetStackFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving stack {Name}.", name);
            return Result.Failure<Stack>(StacksErrors.GetStackFailed);
        }
    }

    public async Task<Result<bool>> StackExistsWithNameAsync(string name)
    {
        _logger.LogInformation("Checking if stack exists with name {Name}.", name);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.StackExistsWithName;
                var exists = await connection.QuerySingleAsync<bool>(sql, new { Name = name });
                _logger.LogInformation("Stack with name {Name} exists: {Exists}.", name, exists);
                return Result.Success(exists);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while checking if stack exists with name {Name}.", name);
            return Result.Failure<bool>(StacksErrors.StackWithNameFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking if stack exists with name {Name}.", name);
            return Result.Failure<bool>(StacksErrors.StackWithNameFailed);
        }
    }

    public async Task<Result<int>> GetCardsCountInStackAsync(int stackId)
    {
        _logger.LogInformation("Getting cards count in stack {StackId}.", stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetCardsCountInStack;
                var count = await connection.QuerySingleAsync<int>(sql, new { StackId = stackId });
                _logger.LogInformation("Stack {StackId} has {Count} cards.", stackId, count);
                return Result.Success(count);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while getting cards count in stack {StackId}.", stackId);
            return Result.Failure<int>(StacksErrors.GetCardsCountFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting cards count in stack {StackId}.", stackId);
            return Result.Failure<int>(StacksErrors.GetCardsCountFailed);
        }
    }

    public async Task<Result> UpdateMultipleChoiceCardAsync
        (
        int multipleChoiceCardId,
        int stackId, string question, List<string> choices, List<string> answers
        )
    {
        _logger.LogInformation("Updating multiple choice card {CardId} in stack {StackId}.", multipleChoiceCardId, stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateMultipleChoiceCardInStack;
                await connection.ExecuteAsync(sql, new
                {
                    Question = question,
                    Choices = string.Join(";", choices),
                    Answer = string.Join(";", answers),
                    Id = multipleChoiceCardId,
                    StackId = stackId
                });
            }
            _logger.LogInformation("Successfully updated multiple choice card {CardId} in stack {StackId}.", multipleChoiceCardId, stackId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating multiple choice card {CardId} in stack {StackId}.", multipleChoiceCardId, stackId);
            return Result.Failure(StacksErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating multiple choice card {CardId} in stack {StackId}.", multipleChoiceCardId, stackId);
            return Result.Failure(StacksErrors.UpdateFailed);
        }
    }

    public async Task<Result> UpdateClozeCardInStackAsync(int clozeCardId, int stackId, string clozeText)
    {
        _logger.LogInformation("Updating cloze card {CardId} in stack {StackId}.", clozeCardId, stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateClozeCardInStack;

                await connection.ExecuteAsync(sql, new
                {
                    ClozeText = clozeText,
                    Id = clozeCardId,
                    StackId = stackId
                });

                _logger.LogInformation("Successfully updated cloze card {CardId} in stack {StackId}.", clozeCardId, stackId);
                return Result.Success();
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating cloze card {CardId} in stack {StackId}.", clozeCardId, stackId);
            return Result.Failure(StacksErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating cloze card {CardId} in stack {StackId}.", clozeCardId, stackId);
            return Result.Failure(StacksErrors.UpdateFailed);
        }
    }

    public async Task<Result<IEnumerable<BaseCard>>> GetCardsToStudyByStackIdAsync(int stackId)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetCardsToStudyByStackId;
                using (var reader = await connection.ExecuteReaderAsync(sql, new { StackId = stackId }))
                {
                    var flashcardParser = reader.GetRowParser<Flashcard>();
                    var clozeCardParser = reader.GetRowParser<ClozeCard>();
                    var fillInCardParser = reader.GetRowParser<FillInCard>();
                    var multipleChoiceCardParser = reader.GetRowParser<MultipleChoiceCard>();

                    var cards = new List<BaseCard>();

                    while (await reader.ReadAsync())
                    {
                        var discriminator = reader.GetString(reader.GetOrdinal("CardType"));
                        if (!Enum.TryParse(discriminator, out CardType cardType))
                        {
                            _logger.LogWarning("Unknown card type discriminator: {Discriminator}", discriminator);
                            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
                        }

                        BaseCard card = cardType switch
                        {
                            CardType.Flashcard => flashcardParser(reader),
                            CardType.Cloze => clozeCardParser(reader),
                            CardType.FillIn => fillInCardParser(reader),
                            CardType.MultipleChoice => multipleChoiceCardParser(reader),
                            _ => throw new InvalidOperationException("Unknown card type")
                        };

                        cards.Add(card);
                    }

                    _logger.LogInformation("Successfully retrieved {Count} cards to study for stack {StackId}.", cards.Count, stackId);
                    return Result.Success(cards.AsEnumerable());
                }
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving cards to study for stack {StackId}.", stackId);
            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving cards to study for stack {StackId}.", stackId);
            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
        }
    }

    public async Task<Result<IEnumerable<StackSummaryDto>>> GetAllStackSummariesAsync()
    {
        _logger.LogInformation("Retrieving all stack summaries.");
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetAllStackSummaries;
                var summaries = await connection.QueryAsync<StackSummaryDto>(sql);
                _logger.LogInformation("Successfully retrieved {Count} stack summaries.", summaries.Count());
                return Result.Success(summaries);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving all stack summaries.");
            return Result.Failure<IEnumerable<StackSummaryDto>>(StacksErrors.GetStacksFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving all stack summaries.");
            return Result.Failure<IEnumerable<StackSummaryDto>>(StacksErrors.GetStacksFailed);
        }
    }
}