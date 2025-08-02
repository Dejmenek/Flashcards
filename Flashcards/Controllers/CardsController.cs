using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Controllers;

public class CardsController
{
    private readonly ICardsService _cardsService;

    public CardsController(ICardsService cardsService)
    {
        _cardsService = cardsService;
    }

    public async Task<Result> AddCardAsync()
    {
        return await _cardsService.AddCardAsync();
    }

    public async Task<Result> DeleteCardAsync()
    {
        return await _cardsService.DeleteCardAsync();
    }

    public async Task<Result<List<BaseCardDTO>>> GetAllCardsAsync()
    {
        return await _cardsService.GetAllCardsAsync();
    }

    public async Task<Result> UpdateCardAsync()
    {
        return await _cardsService.UpdateCardAsync();
    }
}
