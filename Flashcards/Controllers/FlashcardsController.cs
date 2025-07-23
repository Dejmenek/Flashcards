using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Controllers;

public class FlashcardsController
{
    private readonly IFlashcardsService _flashcardsService;

    public FlashcardsController(IFlashcardsService flashcardsService)
    {
        _flashcardsService = flashcardsService;
    }

    public async Task<Result> AddFlashcardAsync()
    {
        return await _flashcardsService.AddFlashcardAsync();
    }

    public async Task<Result> DeleteFlashcardAsync()
    {
        return await _flashcardsService.DeleteFlashcardAsync();
    }

    public async Task<Result<List<FlashcardDTO>>> GetAllFlashcardsAsync()
    {
        return await _flashcardsService.GetAllFlashcardsAsync();
    }

    public async Task<Result> UpdateFlashcardAsync()
    {
        return await _flashcardsService.UpdateFlashcardAsync();
    }
}
