using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Services;

public class CardsService : ICardsService
{
    private readonly ICardsRepository _cardsRepository;
    private readonly IStacksRepository _stacksRepository;
    private readonly IUserInteractionService _userInteractionService;

    public CardsService(ICardsRepository cardsRepository, IUserInteractionService userInteractionService, IStacksRepository stacksRepository)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
    }

    public async Task<Result> AddCardAsync()
    {
        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        if (stacksResult.IsFailure) return Result.Failure(stacksResult.Error);

        if (!stacksResult.Value.Any()) return Result.Failure(StacksErrors.StacksNotFound);

        List<StackDTO> stackDtos = new();
        foreach (var stack in stacksResult.Value)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        string chosenStackName = _userInteractionService.GetStack(stackDtos);
        int chosenStackId = stacksResult.Value.Single(s => s.Name == chosenStackName).Id;

        CardType chosenCardType = _userInteractionService.GetCardType();
        ICardStrategy strategy = chosenCardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService),
            _ => throw new ArgumentOutOfRangeException(nameof(chosenCardType), "Invalid card type selected.")
        };

        return await strategy.AddCardAsync(chosenStackId);
    }

    public async Task<Result> DeleteCardAsync()
    {
        var cardsResult = await GetAllCardsAsync();
        if (cardsResult.IsFailure) return Result.Failure(cardsResult.Error);
        if (!cardsResult.Value.Any()) return Result.Failure(CardsErrors.CardsNotFound);

        BaseCardDTO chosenCard = _userInteractionService.GetCard(cardsResult.Value);

        var deleteResult = await _cardsRepository.DeleteCardAsync(chosenCard.Id);
        if (deleteResult.IsFailure) return Result.Failure(deleteResult.Error);

        return Result.Success();
    }

    public async Task<Result<List<BaseCardDTO>>> GetAllCardsAsync()
    {
        var cardsResult = await _cardsRepository.GetAllCardsAsync();
        if (cardsResult.IsFailure) return Result.Failure<List<BaseCardDTO>>(cardsResult.Error);

        List<BaseCardDTO> cardDtos = new();
        foreach (var card in cardsResult.Value)
        {
            switch (card)
            {
                case Flashcard flashcard:
                    cardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
                    break;
                default:
                    return Result.Failure<List<BaseCardDTO>>(CardsErrors.GetAllFailed);
            }
        }

        return Result.Success(cardDtos);
    }

    public async Task<Result> UpdateCardAsync()
    {
        var cardsResult = await GetAllCardsAsync();
        if (cardsResult.IsFailure) return Result.Failure(cardsResult.Error);

        BaseCardDTO chosenCard = _userInteractionService.GetCard(cardsResult.Value);

        ICardStrategy strategy = chosenCard.CardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService),
            _ => throw new ArgumentOutOfRangeException(nameof(chosenCard.CardType), "Invalid card type selected.")
        };

        return await strategy.UpdateCardAsync(chosenCard.Id);
    }
}
