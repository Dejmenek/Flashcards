using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.Services.Interfaces;
public interface IStacksService
{
    Task<Result> AddStackAsync();
    Task<Result> AddFlashcardToStackAsync();
    Task<Result> DeleteStackAsync();
    Task<Result> DeleteFlashcardFromStackAsync();
    Task<Result> UpdateFlashcardInStackAsync();
    Task<Result<List<FlashcardDTO>>> GetFlashcardsByStackIdAsync();
    Task<Result<int>> GetFlashcardsCountInStackAsync();
    Task<Result<List<StackDTO>>> GetAllStacksAsync();
    Task<Result> GetStackAsync();
    Stack GetCurrentStack();
}
