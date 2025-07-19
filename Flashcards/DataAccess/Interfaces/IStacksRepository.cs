using Flashcards.Models;

namespace Flashcards.DataAccess.Interfaces;

public interface IStacksRepository
{
    Task<Stack> GetStackAsync(string name);
    Task DeleteStackAsync(int stackId);
    Task UpdateFlashcardInStackAsync(int flashcardId, int stackId, string front, string back);
    Task<int> GetFlashcardsCountInStackAsync(int stackId);
    Task DeleteFlashcardFromStackAsync(int flashcardId, int stackId);
    Task<IEnumerable<Flashcard>> GetFlashcardsByStackIdAsync(int stackId);
    Task AddStackAsync(string name);
    Task<IEnumerable<Stack>> GetAllStacksAsync();
    Task<bool> StackExistsWithNameAsync(string name);
    Task<bool> HasStackAsync();
    Task<bool> HasStackAnyFlashcardsAsync(int stackId);
}
