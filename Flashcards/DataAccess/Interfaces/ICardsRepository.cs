using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.DataAccess.Interfaces;

public interface ICardsRepository
{
    Task<Result> AddFlashcardAsync(int stackId, string front, string back);
    Task<Result> AddMultipleChoiceCardAsync(int stackId, string question, List<string> choices, List<string> answers);
    Task<Result> DeleteCardAsync(int cardId);
    Task<Result> UpdateFlashcardAsync(int flashcardId, string front, string back);
    Task<Result> UpdateMultipleChoiceCardAsync(int cardId, string question, List<string> choices, List<string> answers);
    Task<Result<IEnumerable<BaseCard>>> GetAllCardsAsync();
}