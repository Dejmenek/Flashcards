using Flashcards.Models;

namespace Flashcards.Services.Interfaces;
public interface IFlashcardsService
{
    void AddFlashcard();
    void DeleteFlashcard();
    List<FlashcardDTO> GetAllFlashcards();
    void UpdateFlashcard();
}
