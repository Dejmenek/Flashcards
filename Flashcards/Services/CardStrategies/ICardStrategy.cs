using Flashcards.Utils;

namespace Flashcards.Services.CardStrategies;
public interface ICardStrategy
{
    Task<Result> AddCardAsync(int stackId);
    Task<Result> UpdateCardAsync(int cardId);
    Task<Result> UpdateCardInStackAsync(int cardId, int stackId);
}
