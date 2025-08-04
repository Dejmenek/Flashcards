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
                    while (reader.Read())
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

    public async Task<Result> UpdateFlashcardAsync(int flashcardId, string front, string back)
    {
        _logger.LogInformation("Updating flashcard {FlashcardId}.", flashcardId);
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
            _logger.LogInformation("Successfully updated flashcard {FlashcardId}.", flashcardId);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating flashcard {FlashcardId}.", flashcardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating flashcard {FlashcardId}.", flashcardId);
            return Result.Failure(CardsErrors.UpdateFailed);
        }
    }
}
