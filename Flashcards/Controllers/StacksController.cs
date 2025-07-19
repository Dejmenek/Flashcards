using Flashcards.Models;
using Flashcards.Services.Interfaces;

namespace Flashcards.Controllers;

public class StacksController
{
    private readonly IStacksService _stacksService;

    public StacksController(IStacksService stacksService)
    {
        _stacksService = stacksService;
    }

    public async Task AddStackAsync()
    {
        await _stacksService.AddStackAsync();
    }

    public async Task AddFlashcardToStackAsync()
    {
        await _stacksService.AddFlashcardToStackAsync();
    }

    public async Task DeleteStackAsync()
    {
        await _stacksService.DeleteStackAsync();
    }

    public async Task DeleteFlashcardFromStackAsync()
    {
        await _stacksService.DeleteFlashcardFromStackAsync();
    }

    public async Task UpdateFlashcardInStackAsync()
    {
        await _stacksService.UpdateFlashcardInStackAsync();
    }

    public async Task<List<FlashcardDTO>> GetFlashcardsByStackIdAsync()
    {
        return await _stacksService.GetFlashcardsByStackIdAsync();
    }

    public async Task<int> GetFlashcardsCountInStackAsync()
    {
        return await _stacksService.GetFlashcardsCountInStackAsync();
    }

    public async Task<List<StackDTO>> GetAllStacksAsync()
    {
        return await _stacksService.GetAllStacksAsync();
    }

    public async Task GetStackAsync()
    {
        await _stacksService.GetStackAsync();
    }

    public async Task<Stack> GetCurrentStackAsync()
    {
        return await _stacksService.GetCurrentStackAsync();
    }
}
