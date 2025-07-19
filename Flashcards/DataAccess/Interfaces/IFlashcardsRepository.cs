using Flashcards.Models;

namespace Flashcards.DataAccess.Interfaces;

public interface IFlashcardsRepository
{
    Task AddFlashcardAsync(int stackId, string front, string back);
    Task DeleteFlashcardAsync(int flashcardId);
    Task UpdateFlashcardAsync(int flashcardId, string front, string back);
    Task<IEnumerable<Flashcard>> GetAllFlashcardsAsync();
}
