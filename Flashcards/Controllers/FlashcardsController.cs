using Flashcards.Models;
using Flashcards.Services.Interfaces;

namespace Flashcards.Controllers;

public class FlashcardsController
{
    private readonly IFlashcardsService _flashcardsService;

    public FlashcardsController(IFlashcardsService flashcardsService)
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
