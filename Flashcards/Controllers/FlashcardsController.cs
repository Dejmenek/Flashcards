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

    public async Task AddFlashcardAsync()
    {
        await _flashcardsService.AddFlashcardAsync();
    }

    public async Task DeleteFlashcardAsync()
    {
        await _flashcardsService.DeleteFlashcardAsync();
    }

    public async Task<List<FlashcardDTO>> GetAllFlashcardsAsync()
    {
        return await _flashcardsService.GetAllFlashcardsAsync();
    }

    public async Task UpdateFlashcardAsync()
    {
        await _flashcardsService.UpdateFlashcardAsync();
    }
}
