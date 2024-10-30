using Flashcards.Models;

namespace Flashcards.DataAccess.Interfaces;

public interface IFlashcardsRepository
{
    void AddFlashcard(int stackId, string front, string back);
    void DeleteFlashcard(int flashcardId);
    void UpdateFlashcard(int flashcardId, string front, string back);
    IEnumerable<Flashcard> GetAllFlashcards();
}
