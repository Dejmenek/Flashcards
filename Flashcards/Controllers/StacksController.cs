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

    public async Task<Result> AddCardToStackAsync()
    {
        return await _stacksService.AddCardToStackAsync();
    }

    public async Task<Result> DeleteStackAsync()
    {
        return await _stacksService.DeleteStackAsync();
    }

    public async Task<Result> DeleteCardFromStackAsync()
    {
        return await _stacksService.DeleteCardFromStackAsync();
    }

    public async Task<Result> UpdateCardInStackAsync()
    {
        return await _stacksService.UpdateCardInStackAsync();
    }

    public async Task<Result<List<BaseCardDto>>> GetCardsByStackIdAsync()
    {
        return await _stacksService.GetCardsByStackIdAsync();
    }

    public async Task<Result<List<BaseCardDto>>> GetCardsToStudyByStackIdAsync()
    {
        return await _stacksService.GetCardsToStudyByStackIdAsync();
    }

    public async Task<Result<int>> GetCardsCountInStackAsync()
    {
        return await _stacksService.GetCardsCountInStackAsync();
    }

    public async Task<Result<List<StackDto>>> GetAllStacksAsync()
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