using Flashcards.DataAccess.Interfaces;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Services.CardStrategies;
public class FlashcardStrategy : ICardStrategy
{
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository? _stacksRepository;

    public FlashcardStrategy(ICardsRepository cardsRepository, IUserInteractionService userInteractionService)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
    }

    public FlashcardStrategy(ICardsRepository flashcardsRepository, IUserInteractionService userInteractionService, IStacksRepository stacksRepository)
    {
        _cardsRepository = flashcardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
    }

    public async Task<Result> AddCardAsync(int stackId)
    {
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        var addResult = await _cardsRepository.AddFlashcardAsync(stackId, front, back);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardAsync(int cardId)
    {
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        var updateResult = await _cardsRepository.UpdateFlashcardAsync(cardId, front, back);
        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardInStackAsync(int cardId, int stackId)
    {
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        var updateResult = await _stacksRepository.UpdateFlashcardInStackAsync(cardId, stackId, front, back);
        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }
}
