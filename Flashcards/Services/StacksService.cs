using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using Spectre.Console;

namespace Flashcards.Services;

public class StacksService : IStacksService
{
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    public Stack? CurrentStack { get; private set; }

    public StacksService(IStacksRepository stacksRepository, IUserInteractionService userInteractionService, ICardsRepository cardsRepository)
    {
        _stacksRepository = stacksRepository;
        _userInteractionService = userInteractionService;
        _cardsRepository = cardsRepository;
    }

    public async Task<Result> AddStackAsync()
    {
        string name = _userInteractionService.GetStackName();

        while (true)
        {
            var existsResult = await _stacksRepository.StackExistsWithNameAsync(name);
            if (existsResult.IsFailure) return Result.Failure(existsResult.Error);
            if (!existsResult.Value) break;

            AnsiConsole.MarkupLine($"There is already a stack named {name}. Please try a different name.");
            name = _userInteractionService.GetStackName();
        }

        var addResult = await _stacksRepository.AddStackAsync(name);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> AddCardToStackAsync()
    {
        if (CurrentStack == null)
            return Result.Failure(StacksErrors.CurrentStackNotFound);

        CardType chosenCardType = _userInteractionService.GetCardType();
        ICardStrategy strategy = chosenCardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService),
            _ => throw new ArgumentOutOfRangeException(nameof(chosenCardType), "Invalid card type selected.")
        };

        return await strategy.AddCardAsync(CurrentStack.Id);
    }

    public async Task<Result> DeleteStackAsync()
    {
        if (CurrentStack == null)
            return Result.Failure(StacksErrors.CurrentStackNotFound);

        var deleteResult = await _stacksRepository.DeleteStackAsync(CurrentStack.Id);
        if (deleteResult.IsFailure) return Result.Failure(deleteResult.Error);

        return Result.Success();
    }

    public async Task<Result> DeleteCardFromStackAsync()
    {
        var cardsResult = await GetCardsByStackIdAsync();
        if (cardsResult.IsFailure) return Result.Failure(cardsResult.Error);

        if (cardsResult.Value.Count == 0)
            return Result.Failure(CardsErrors.CardsNotFound);

        BaseCardDTO chosenCard = _userInteractionService.GetCard(cardsResult.Value);

        var deleteResult = await _stacksRepository.DeleteCardFromStackAsync(chosenCard.Id, CurrentStack!.Id);
        if (deleteResult.IsFailure) return Result.Failure(deleteResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardInStackAsync()
    {
        var cardsResult = await GetCardsByStackIdAsync();
        if (cardsResult.IsFailure) return Result.Failure(cardsResult.Error);

        if (cardsResult.Value.Count == 0)
            return Result.Failure(CardsErrors.CardsNotFound);

        BaseCardDTO chosenCard = _userInteractionService.GetCard(cardsResult.Value);

        ICardStrategy strategy = chosenCard.CardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService, _stacksRepository),
            _ => throw new ArgumentOutOfRangeException(nameof(chosenCard.CardType), "Invalid card type selected.")
        };

        return await strategy.UpdateCardInStackAsync(chosenCard.Id, CurrentStack!.Id);
    }

    public async Task<Result<List<BaseCardDTO>>> GetCardsByStackIdAsync()
    {
        if (CurrentStack == null)
            return Result.Failure<List<BaseCardDTO>>(StacksErrors.CurrentStackNotFound);

        var cardsResult = await _stacksRepository.GetCardsByStackIdAsync(CurrentStack.Id);
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

    public async Task<Result<int>> GetCardsCountInStackAsync()
    {
        if (CurrentStack == null)
            return Result.Failure<int>(StacksErrors.CurrentStackNotFound);

        var countResult = await _stacksRepository.GetCardsCountInStackAsync(CurrentStack.Id);
        if (countResult.IsFailure) return Result.Failure<int>(countResult.Error);

        return Result.Success(countResult.Value);
    }

    public async Task<Result<List<StackDTO>>> GetAllStacksAsync()
    {
        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        if (stacksResult.IsFailure) return Result.Failure<List<StackDTO>>(stacksResult.Error);

        List<StackDTO> stackDtos = new();
        foreach (var stack in stacksResult.Value)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        return Result.Success(stackDtos);
    }

    public async Task<Result> GetStackAsync()
    {
        var stacksResult = await GetAllStacksAsync();
        if (stacksResult.IsFailure) return Result.Failure(stacksResult.Error);

        if (stacksResult.Value.Count == 0)
            return Result.Failure(StacksErrors.StacksNotFound);

        string name = _userInteractionService.GetStack(stacksResult.Value);

        var stackResult = await _stacksRepository.GetStackAsync(name);
        if (stackResult.IsFailure) return Result.Failure(stackResult.Error);

        CurrentStack = stackResult.Value;
        return Result.Success();
    }

    public Stack GetCurrentStack()
    {
        return CurrentStack!;
    }
}
