using Dapper;

using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flashcards.DataAccess.Repositories;

public class CardsRepository : ICardsRepository
{
    private readonly string _defaultConnectionString;
    private readonly ILogger<CardsRepository> _logger;

    public CardsRepository(IConfiguration config, ILogger<CardsRepository> logger)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
        _logger = logger;
    }

    public async Task<Result> AddClozeCardAsync(int stackId, string clozeText)
    {
        _logger.LogInformation("Adding cloze card to stack {StackId}.", stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddClozeCard;

                await connection.ExecuteAsync(sql, new
                {
                    StackId = stackId,
                    ClozeText = clozeText,
                    CardType = CardType.Cloze.ToString()
                });

                _logger.LogInformation("Successfully added cloze card to stack {StackId}.", stackId);
                return Result.Success();
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while adding cloze card to stack {StackId}.", stackId);
            return Result.Failure(CardsErrors.AddFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding cloze card to stack {StackId}.", stackId);
            return Result.Failure(CardsErrors.AddFailed);
        }
    }

    public async Task<Result> AddFlashcardAsync(int stackId, string front, string back)
    {
        _logger.LogInformation("Adding flashcard to stack {StackId}.", stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddFlashcard;

                await connection.ExecuteAsync(sql, new
                {
                    StackId = stackId,
                    Front = front,
                    Back = back,
                    CardType = CardType.Flashcard.ToString()
                });
            }
            _logger.LogInformation("Successfully added flashcard to stack {StackId}.", stackId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while adding flashcard to stack {StackId}.", stackId);
            return Result.Failure(CardsErrors.AddFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding flashcard to stack {StackId}.", stackId);
            return Result.Failure(CardsErrors.AddFailed);
        }
    }

    public async Task<Result> AddMultipleChoiceCardAsync(int stackId, string question, List<string> choices, List<string> answers)
    {
        _logger.LogInformation("Adding multiple choice card to stack {StackId}.", stackId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddMultipleChoiceCard;
                await connection.ExecuteAsync(sql, new
                {
                    StackId = stackId,
                    Question = question,
                    Choices = string.Join(";", choices),
                    Answer = string.Join(";", answers),
                    CardType = CardType.MultipleChoice.ToString()
                });
                _logger.LogInformation("Successfully added multiple choice card to stack {StackId}.", stackId);
                return Result.Success();
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while adding multiple choice card to stack {StackId}.", stackId);
            return Result.Failure(CardsErrors.AddFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding multiple choice card to stack {StackId}.", stackId);
            return Result.Failure(CardsErrors.AddFailed);
        }
    }

    public async Task<Result> DeleteCardAsync(int cardId)
    {
        _logger.LogInformation("Deleting card {CardId}.", cardId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.DeleteCard;

                await connection.ExecuteAsync(sql, new
                {
                    Id = cardId
                });
            }
            _logger.LogInformation("Successfully deleted card {CardId}.", cardId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while deleting card {CardId}.", cardId);
            return Result.Failure(CardsErrors.DeleteFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting card {CardId}.", cardId);
            return Result.Failure(CardsErrors.DeleteFailed);
        }
    }

    public async Task<Result<IEnumerable<BaseCard>>> GetAllCardsAsync()
    {
        _logger.LogInformation("Retrieving all cards.");
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetCards;
                using (var reader = await connection.ExecuteReaderAsync(sql))
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

                    _logger.LogInformation("Successfully retrieved {Count} cards.", cards.Count);
                    return Result.Success(cards.AsEnumerable());
                }
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving all cards.");
            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving all cards.");
            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
        }
    }

    public async Task<Result> UpdateCardProgress(int cardId, int newBox, DateTime nextReviewDate)
    {
        _logger.LogInformation("Updating progress for card {CardId}.", cardId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateCardProgress;
                await connection.ExecuteAsync(sql, new
                {
                    Box = newBox,
                    NextReviewDate = nextReviewDate,
                    Id = cardId
                });
            }

            _logger.LogInformation("Successfully updated progress for card {CardId}.", cardId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating progress for card {CardId}.", cardId);
            return Result.Failure(CardsErrors.UpdateCardProgressFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating progress for card {CardId}.", cardId);
            return Result.Failure(CardsErrors.UpdateCardProgressFailed);
        }
    }

    public async Task<Result> UpdateClozeCardAsync(int clozeCardId, string clozeText)
    {
        _logger.LogInformation("Updating cloze card {CardId}.", clozeCardId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateClozeCard;

                await connection.ExecuteAsync(sql, new
                {
                    ClozeText = clozeText,
                    Id = clozeCardId
                });

                _logger.LogInformation("Successfully updated cloze card {CardId}.", clozeCardId);
                return Result.Success();
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating cloze card {CardId}.", clozeCardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating cloze card {CardId}.", clozeCardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
    }

    public async Task<Result> UpdateFlashcardAsync(int flashcardId, string front, string back)
    {
        _logger.LogInformation("Updating flashcard {CardId}.", flashcardId);
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
            _logger.LogInformation("Successfully updated flashcard {CardId}.", flashcardId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating flashcard {CardId}.", flashcardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating flashcard {CardId}.", flashcardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
    }

    public async Task<Result> UpdateMultipleChoiceCardAsync(int multipleChoiceCardId, string question, List<string> choices, List<string> answers)
    {
        _logger.LogInformation("Updating multiple choice card {CardId}.", multipleChoiceCardId);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.UpdateMultipleChoiceCard;

                await connection.ExecuteAsync(sql, new
                {
                    Question = question,
                    Choices = string.Join(";", choices),
                    Answer = string.Join(";", answers),
                    Id = multipleChoiceCardId
                });
            }
            _logger.LogInformation("Successfully updated multiple choice card {CardId}.", multipleChoiceCardId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating multiple choice card {CardId}.", multipleChoiceCardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating multiple choice card {CardId}.", multipleChoiceCardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
    }
}