using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace Flashcards.Services;

public class StacksService : IStacksService
{
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly ILogger<StacksService> _logger;
    public Stack? CurrentStack { get; private set; }

    public StacksService(
        IStacksRepository stacksRepository,
        IUserInteractionService userInteractionService,
        ICardsRepository cardsRepository,
        ILogger<StacksService> logger)
    {
        _stacksRepository = stacksRepository;
        _userInteractionService = userInteractionService;
        _cardsRepository = cardsRepository;
        _logger = logger;
    }

    public async Task<Result> AddStackAsync()
    {
        _logger.LogInformation("Starting AddStackAsync.");
        string name = _userInteractionService.GetStackName();

        while (true)
        {
            var existsResult = await _stacksRepository.StackExistsWithNameAsync(name);
            if (existsResult.IsFailure)
            {
                _logger.LogWarning("Failed to check if stack exists: {Error}", existsResult.Error.Description);
                return Result.Failure(existsResult.Error);
            }
            if (!existsResult.Value) break;

            AnsiConsole.MarkupLine($"There is already a stack named {name}. Please try a different name.");
            name = _userInteractionService.GetStackName();
        }

        var addResult = await _stacksRepository.AddStackAsync(name);
        if (addResult.IsFailure)
        {
            _logger.LogWarning("Failed to add stack {Name}: {Error}", name, addResult.Error.Description);
            return Result.Failure(addResult.Error);
        }

        _logger.LogInformation("Stack {Name} added successfully.", name);
        return Result.Success();
    }

    public async Task<Result> AddCardToStackAsync()
    {
        _logger.LogInformation("Starting AddCardToStackAsync.");
        if (CurrentStack == null)
        {
            _logger.LogWarning("No current stack selected.");
            return Result.Failure(StacksErrors.CurrentStackNotFound);
        }

        CardType chosenCardType = _userInteractionService.GetCardType();

        ICardStrategy strategy = chosenCardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService),
            CardType.MultipleChoice => new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService),
            CardType.Cloze => new ClozeCardStrategy(_cardsRepository, _userInteractionService),
            _ => throw new InvalidOperationException($"Unsupported card type: {chosenCardType}")
        };

        var result = await strategy.AddCardAsync(CurrentStack.Id);
        if (result.IsSuccess)
            _logger.LogInformation("Card added successfully to stack {StackId}.", CurrentStack.Id);
        else
            _logger.LogWarning("Failed to add card to stack {StackId}: {Error}", CurrentStack.Id, result.Error.Description);

        return result;
    }

    public async Task<Result> DeleteStackAsync()
    {
        _logger.LogInformation("Starting DeleteStackAsync.");
        if (CurrentStack == null)
        {
            _logger.LogWarning("No current stack selected.");
            return Result.Failure(StacksErrors.CurrentStackNotFound);
        }

        var deleteResult = await _stacksRepository.DeleteStackAsync(CurrentStack.Id);
        if (deleteResult.IsFailure)
        {
            _logger.LogWarning("Failed to delete stack {StackId}: {Error}", CurrentStack.Id, deleteResult.Error.Description);
            return Result.Failure(deleteResult.Error);
        }

        _logger.LogInformation("Stack {StackId} deleted successfully.", CurrentStack.Id);
        return Result.Success();
    }

    public async Task<Result> DeleteCardFromStackAsync()
    {
        _logger.LogInformation("Starting DeleteCardFromStackAsync.");
        var cardsResult = await GetCardsByStackIdAsync();
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards for stack: {Error}", cardsResult.Error.Description);
            return Result.Failure(cardsResult.Error);
        }

        if (cardsResult.Value.Count == 0)
        {
            _logger.LogWarning("No cards found in current stack to delete.");
            return Result.Failure(CardsErrors.CardsNotFound);
        }

        BaseCardDto chosenCard = _userInteractionService.GetCard(cardsResult.Value);
        _logger.LogInformation("User selected card ID {CardId} for deletion from stack {StackId}.", chosenCard.Id, CurrentStack!.Id);

        var deleteResult = await _stacksRepository.DeleteCardFromStackAsync(chosenCard.Id, CurrentStack!.Id);
        if (deleteResult.IsFailure)
        {
            _logger.LogWarning("Failed to delete card {CardId} from stack {StackId}: {Error}", chosenCard.Id, CurrentStack.Id, deleteResult.Error.Description);
            return Result.Failure(deleteResult.Error);
        }

        _logger.LogInformation("Card {CardId} deleted successfully from stack {StackId}.", chosenCard.Id, CurrentStack.Id);
        return Result.Success();
    }

    public async Task<Result> UpdateCardInStackAsync()
    {
        _logger.LogInformation("Starting UpdateCardInStackAsync.");
        var cardsResult = await GetCardsByStackIdAsync();
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards for update: {Error}", cardsResult.Error.Description);
            return Result.Failure(cardsResult.Error);
        }

        if (cardsResult.Value.Count == 0)
        {
            _logger.LogWarning("No cards found in current stack to update.");
            return Result.Failure(CardsErrors.CardsNotFound);
        }

        BaseCardDto chosenCard = _userInteractionService.GetCard(cardsResult.Value);
        _logger.LogInformation("User selected card ID {CardId} for update in stack {StackId}.", chosenCard.Id, CurrentStack!.Id);

        ICardStrategy strategy = chosenCard.CardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService, _stacksRepository),
            CardType.MultipleChoice => new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService, _stacksRepository),
            CardType.Cloze => new ClozeCardStrategy(_cardsRepository, _userInteractionService, _stacksRepository),
            _ => throw new InvalidOperationException($"Unsupported card type: {chosenCard.CardType}")
        };

        var result = await strategy.UpdateCardInStackAsync(chosenCard.Id, CurrentStack!.Id);
        if (result.IsSuccess)
            _logger.LogInformation("Card {CardId} updated successfully in stack {StackId}.", chosenCard.Id, CurrentStack.Id);
        else
            _logger.LogWarning("Failed to update card {CardId} in stack {StackId}: {Error}", chosenCard.Id, CurrentStack.Id, result.Error.Description);

        return result;
    }

    public async Task<Result<List<BaseCardDto>>> GetCardsByStackIdAsync()
    {
        _logger.LogInformation("Starting GetCardsByStackIdAsync.");
        if (CurrentStack == null)
        {
            _logger.LogWarning("No current stack selected.");
            return Result.Failure<List<BaseCardDto>>(StacksErrors.CurrentStackNotFound);
        }

        var cardsResult = await _stacksRepository.GetCardsByStackIdAsync(CurrentStack.Id);
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards for stack {StackId}: {Error}", CurrentStack.Id, cardsResult.Error.Description);
            return Result.Failure<List<BaseCardDto>>(cardsResult.Error);
        }

        List<BaseCardDto> cardDtos = new();
        foreach (var card in cardsResult.Value)
        {
            switch (card)
            {
                case Flashcard flashcard:
                    cardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
                    break;
                case MultipleChoiceCard multipleChoiceCard:
                    cardDtos.Add(Mapper.ToMultipleChoiceCardDTO(multipleChoiceCard));
                    break;
                case ClozeCard clozeCard:
                    cardDtos.Add(Mapper.ToClozeCardDTO(clozeCard));
                    break;
                default:
                    _logger.LogWarning("Unknown card type encountered in GetCardsByStackIdAsync.");
                    return Result.Failure<List<BaseCardDto>>(CardsErrors.GetAllFailed);
            }
        }

        _logger.LogInformation("Retrieved {Count} cards for stack {StackId}.", cardDtos.Count, CurrentStack.Id);
        return Result.Success(cardDtos);
    }

    public async Task<Result<int>> GetCardsCountInStackAsync()
    {
        _logger.LogInformation("Starting GetCardsCountInStackAsync.");
        if (CurrentStack == null)
        {
            _logger.LogWarning("No current stack selected.");
            return Result.Failure<int>(StacksErrors.CurrentStackNotFound);
        }

        var countResult = await _stacksRepository.GetCardsCountInStackAsync(CurrentStack.Id);
        if (countResult.IsFailure)
        {
            _logger.LogWarning("Failed to get cards count for stack {StackId}: {Error}", CurrentStack.Id, countResult.Error.Description);
            return Result.Failure<int>(countResult.Error);
        }

        _logger.LogInformation("Stack {StackId} has {Count} cards.", CurrentStack.Id, countResult.Value);
        return Result.Success(countResult.Value);
    }

    public async Task<Result<List<StackDto>>> GetAllStacksAsync()
    {
        _logger.LogInformation("Starting GetAllStacksAsync.");
        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        if (stacksResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stacks: {Error}", stacksResult.Error.Description);
            return Result.Failure<List<StackDto>>(stacksResult.Error);
        }

        List<StackDto> stackDtos = new();
        foreach (var stack in stacksResult.Value)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        _logger.LogInformation("Retrieved {Count} stacks.", stackDtos.Count);
        return Result.Success(stackDtos);
    }

    public async Task<Result<List<StackSummaryDto>>> GetAllStackSummariesAsync()
    {
        _logger.LogInformation("Starting GetAllStackSummariesAsync.");
        var stacksResult = await _stacksRepository.GetAllStackSummariesAsync();
        if (stacksResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stack summaries: {Error}", stacksResult.Error.Description);
            return Result.Failure<List<StackSummaryDto>>(stacksResult.Error);
        }

        _logger.LogInformation("Retrieved {Count} stack summaries.", stacksResult.Value.Count());
        return Result.Success(stacksResult.Value.ToList());
    }

    public async Task<Result> GetStackAsync()
    {
        _logger.LogInformation("Starting GetStackAsync.");
        var stacksResult = await GetAllStacksAsync();
        if (stacksResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stacks for selection: {Error}", stacksResult.Error.Description);
            return Result.Failure(stacksResult.Error);
        }

        if (stacksResult.Value.Count == 0)
        {
            _logger.LogWarning("No stacks found for selection.");
            return Result.Failure(StacksErrors.StacksNotFound);
        }

        string name = _userInteractionService.GetStack(stacksResult.Value);
        _logger.LogInformation("User selected stack {StackName}.", name);

        var stackResult = await _stacksRepository.GetStackAsync(name);
        if (stackResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stack {StackName}: {Error}", name, stackResult.Error.Description);
            return Result.Failure(stackResult.Error);
        }

        CurrentStack = stackResult.Value;
        _logger.LogInformation("Current stack set to {StackName} (ID: {StackId}).", CurrentStack.Name, CurrentStack.Id);
        return Result.Success();
    }

    public Stack GetCurrentStack()
    {
        return CurrentStack!;
    }

    public async Task<Result<List<BaseCardDto>>> GetCardsToStudyByStackIdAsync()
    {
        _logger.LogInformation("Starting GetCardsToStudyByStackIdAsync.");
        if (CurrentStack == null)
        {
            _logger.LogWarning("No current stack selected.");
            return Result.Failure<List<BaseCardDto>>(StacksErrors.CurrentStackNotFound);
        }

        var cardsResult = await _stacksRepository.GetCardsToStudyByStackIdAsync(CurrentStack.Id);
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards for stack {StackId}: {Error}", CurrentStack.Id, cardsResult.Error.Description);
            return Result.Failure<List<BaseCardDto>>(cardsResult.Error);
        }

        List<BaseCardDto> cardDtos = new();
        foreach (var card in cardsResult.Value)
        {
            switch (card)
            {
                case Flashcard flashcard:
                    cardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
                    break;
                case MultipleChoiceCard multipleChoiceCard:
                    cardDtos.Add(Mapper.ToMultipleChoiceCardDTO(multipleChoiceCard));
                    break;
                case ClozeCard clozeCard:
                    cardDtos.Add(Mapper.ToClozeCardDTO(clozeCard));
                    break;
                default:
                    _logger.LogWarning("Unknown card type encountered in GetCardsByStackIdAsync.");
                    return Result.Failure<List<BaseCardDto>>(CardsErrors.GetAllFailed);
            }
        }

        _logger.LogInformation("Retrieved {Count} cards to study for stack {StackId}.", cardDtos.Count, CurrentStack.Id);
        return Result.Success(cardDtos);
    }

    public async Task<Result> GetStackToStudyAsync()
    {
        _logger.LogInformation("Starting GetStackToStudyAsync.");
        var stacksResult = await GetAllStackSummariesAsync();
        if (stacksResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stacks for selection: {Error}", stacksResult.Error.Description);
            return Result.Failure(stacksResult.Error);
        }

        if (stacksResult.Value.Count == 0)
        {
            _logger.LogWarning("No stacks found for selection.");
            return Result.Failure(StacksErrors.StacksNotFound);
        }

        string name = _userInteractionService.GetStack(stacksResult.Value);
        _logger.LogInformation("User selected stack {StackName}.", name);

        var stackResult = await _stacksRepository.GetStackAsync(name);
        if (stackResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stack {StackName}: {Error}", name, stackResult.Error.Description);
            return Result.Failure(stackResult.Error);
        }

        CurrentStack = stackResult.Value;
        _logger.LogInformation("Current stack set to {StackName} (ID: {StackId}).", CurrentStack.Name, CurrentStack.Id);
        return Result.Success();
    }
}