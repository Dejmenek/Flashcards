using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Flashcards.DataAccess.Repositories;

public class CardsRepository : ICardsRepository
{
    private readonly string _defaultConnectionString;

    public CardsRepository(IConfiguration config)
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
                    Back = back,
                    CardType = CardType.Flashcard.ToString()
                });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(CardsErrors.AddFailed);
        }
        catch (Exception)
        {
            return Result.Failure(CardsErrors.AddFailed);
        }
    }

    public async Task<Result> DeleteCardAsync(int cardId)
    {
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
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(CardsErrors.DeleteFailed);
        }
        catch (Exception)
        {
            return Result.Failure(CardsErrors.DeleteFailed);
        }
    }

    public async Task<Result<IEnumerable<BaseCard>>> GetAllCardsAsync()
    {
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

                    return Result.Success(cards.AsEnumerable());
                }
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed);
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
            return Result.Failure(CardsErrors.UpdateFailed);
        }
        catch (Exception)
        {
            return Result.Failure(CardsErrors.UpdateFailed);
        }
    }
}
