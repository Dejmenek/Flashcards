using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.Services.Interfaces;
public interface IFlashcardsService
{
    Task<Result> AddFlashcardAsync();
    Task<Result> DeleteFlashcardAsync();
    Task<Result<List<FlashcardDTO>>> GetAllFlashcardsAsync();
    Task<Result> UpdateFlashcardAsync();
}
