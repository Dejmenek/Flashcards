using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.DataAccess.Interfaces;

public interface ICardsRepository
{
    Task<Result> AddFlashcardAsync(int stackId, string front, string back);
    Task<Result> AddClozeCardAsync(int stackId, string clozeText);
    Task<Result> AddMultipleChoiceCardAsync(int stackId, string question, List<string> choices, List<string> answers);
    Task<Result> DeleteCardAsync(int cardId);
    Task<Result> UpdateFlashcardAsync(int flashcardId, string front, string back);
    Task<Result> UpdateClozeCardAsync(int clozeCardId, string clozeText);
    Task<Result> UpdateMultipleChoiceCardAsync(int multipleChoiceCardId, string question, List<string> choices, List<string> answers);
    Task<Result<IEnumerable<BaseCard>>> GetAllCardsAsync();
    Task<Result> UpdateCardsProgressBulkAsync(IEnumerable<CardProgressUpdateDto> updates);
}