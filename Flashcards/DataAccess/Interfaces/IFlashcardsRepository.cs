using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.DataAccess.Interfaces;

public interface IFlashcardsRepository
{
    Task<Result> AddFlashcardAsync(int stackId, string front, string back);
    Task<Result> DeleteFlashcardAsync(int flashcardId);
    Task<Result> UpdateFlashcardAsync(int flashcardId, string front, string back);
    Task<Result<IEnumerable<Flashcard>>> GetAllFlashcardsAsync();
}
