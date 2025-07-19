using Flashcards.Models;

namespace Flashcards.Services.Interfaces;
public interface IStacksService
{
    Task AddStackAsync();
    Task AddFlashcardToStackAsync();
    Task DeleteStackAsync();
    Task DeleteFlashcardFromStackAsync();
    Task UpdateFlashcardInStackAsync();
    Task<List<FlashcardDTO>> GetFlashcardsByStackIdAsync();
    Task<int> GetFlashcardsCountInStackAsync();
    Task<List<StackDTO>> GetAllStacksAsync();
    Task GetStackAsync();
    Task<Stack> GetCurrentStackAsync();
}
