using Flashcards.Models;

namespace Flashcards.Services.Interfaces;
public interface IFlashcardsService
{
    Task AddFlashcardAsync();
    Task DeleteFlashcardAsync();
    Task<List<FlashcardDTO>> GetAllFlashcardsAsync();
    Task UpdateFlashcardAsync();
}
