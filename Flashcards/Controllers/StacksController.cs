using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Controllers;

public class StacksController
{
    private readonly IStacksService _stacksService;

    public StacksController(IStacksService stacksService)
    {
        _stacksService = stacksService;
    }

    public async Task<Result> AddStackAsync()
    {
        return await _stacksService.AddStackAsync();
    }

    public async Task<Result> AddFlashcardToStackAsync()
    {
        return await _stacksService.AddFlashcardToStackAsync();
    }

    public async Task<Result> DeleteStackAsync()
    {
        return await _stacksService.DeleteStackAsync();
    }

    public async Task<Result> DeleteFlashcardFromStackAsync()
    {
        return await _stacksService.DeleteFlashcardFromStackAsync();
    }

    public async Task<Result> UpdateFlashcardInStackAsync()
    {
        return await _stacksService.UpdateFlashcardInStackAsync();
    }

    public async Task<Result<List<FlashcardDTO>>> GetFlashcardsByStackIdAsync()
    {
        return await _stacksService.GetFlashcardsByStackIdAsync();
    }

    public async Task<Result<int>> GetFlashcardsCountInStackAsync()
    {
        return await _stacksService.GetFlashcardsCountInStackAsync();
    }

    public async Task<Result<List<StackDTO>>> GetAllStacksAsync()
    {
        return await _stacksService.GetAllStacksAsync();
    }

    public async Task<Result> GetStackAsync()
    {
        return await _stacksService.GetStackAsync();
    }

    public Stack GetCurrentStack()
    {
        return _stacksService.GetCurrentStack();
    }
}
