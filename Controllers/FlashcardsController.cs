using Flashcards.Models;
using Flashcards.Services;

namespace Flashcards.Controllers;

public class FlashcardsController
{
    private readonly FlashcardsService _flashcardsService;

    public FlashcardsController(FlashcardsService flashcardsService)
    {
        _flashcardsService = flashcardsService;
    }

    public void AddFlashcard()
    {
        _flashcardsService.AddFlashcard();
    }

    public void DeleteFlashcard()
    {
        _flashcardsService.DeleteFlashcard();
    }

    public List<FlashcardDTO> GetAllFlashcards()
    {
        return _flashcardsService.GetAllFlashcards();
    }

    public void UpdateFlashcard()
    {
        _flashcardsService.UpdateFlashcard();
    }
}
