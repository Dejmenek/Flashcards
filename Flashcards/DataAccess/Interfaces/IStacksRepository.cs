using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.DataAccess.Interfaces;

public interface IStacksRepository
{
    Task<Result<Stack>> GetStackAsync(string name);
    Task<Result> DeleteStackAsync(int stackId);
    Task<Result> UpdateFlashcardInStackAsync(int flashcardId, int stackId, string front, string back);
    Task<Result<int>> GetFlashcardsCountInStackAsync(int stackId);
    Task<Result> DeleteFlashcardFromStackAsync(int flashcardId, int stackId);
    Task<Result<IEnumerable<Flashcard>>> GetFlashcardsByStackIdAsync(int stackId);
    Task<Result> AddStackAsync(string name);
    Task<Result<IEnumerable<Stack>>> GetAllStacksAsync();
    Task<Result<bool>> StackExistsWithNameAsync(string name);
    Task<Result<bool>> HasStackAnyFlashcardsAsync(int stackId);
}
