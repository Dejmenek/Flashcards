using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Microsoft.Extensions.Logging;

namespace Flashcards.Services;

public class CardsService : ICardsService
{
    private readonly ICardsRepository _cardsRepository;
    private readonly IStacksRepository _stacksRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly ICardStrategyFactory _cardStrategyFactory;
    private readonly ILogger<CardsService> _logger;

    public CardsService(
        ICardsRepository cardsRepository,
        IUserInteractionService userInteractionService,
        IStacksRepository stacksRepository,
        ICardStrategyFactory cardStrategyFactory,
        ILogger<CardsService> logger)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
        _cardStrategyFactory = cardStrategyFactory;
        _logger = logger;
    }

    public async Task<Result> AddCardAsync()
    {
        _logger.LogInformation("Starting AddCardAsync.");

        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        if (stacksResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve stacks: {Error}", stacksResult.Error.Description);
            return Result.Failure(stacksResult.Error);
        }

        if (!stacksResult.Value.Any())
        {
            _logger.LogWarning("No stacks found when adding a card.");
            return Result.Failure(StacksErrors.StacksNotFound);
        }

        List<StackDto> stackDtos = Mapper.ToStackDTOList(stacksResult.Value);

        string chosenStackName = _userInteractionService.GetStack(stackDtos);
        int chosenStackId = stacksResult.Value.Single(s => s.Name == chosenStackName).Id;

        CardType chosenCardType = _userInteractionService.GetCardType();
        _logger.LogInformation("User selected stack {StackName} (ID: {StackId}) and card type {CardType}.", chosenStackName, chosenStackId, chosenCardType);

        ICardStrategy strategy = _cardStrategyFactory.GetCardStrategy(chosenCardType);

        var result = await strategy.AddCardAsync(chosenStackId);
        if (result.IsSuccess)
            _logger.LogInformation("Card added successfully to stack {StackId}.", chosenStackId);
        else
            _logger.LogWarning("Failed to add card to stack {StackId}: {Error}", chosenStackId, result.Error.Description);

        return result;
    }

    public async Task<Result> DeleteCardAsync()
    {
        _logger.LogInformation("Starting DeleteCardAsync.");

        var cardsResult = await GetAllCardsAsync();
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards: {Error}", cardsResult.Error.Description);
            return Result.Failure(cardsResult.Error);
        }
        if (!cardsResult.Value.Any())
        {
            _logger.LogWarning("No cards found to delete.");
            return Result.Failure(CardsErrors.CardsNotFound);
        }

        BaseCardDto chosenCard = _userInteractionService.GetCard(cardsResult.Value);
        _logger.LogInformation("User selected card ID {CardId} for deletion.", chosenCard.Id);

        var deleteResult = await _cardsRepository.DeleteCardAsync(chosenCard.Id);
        if (deleteResult.IsSuccess)
            _logger.LogInformation("Card {CardId} deleted successfully.", chosenCard.Id);
        else
            _logger.LogWarning("Failed to delete card {CardId}: {Error}", chosenCard.Id, deleteResult.Error.Description);

        return deleteResult.IsSuccess ? Result.Success() : Result.Failure(deleteResult.Error);
    }

    public async Task<Result<List<BaseCardDto>>> GetAllCardsAsync()
    {
        _logger.LogInformation("Starting GetAllCardsAsync.");

        var cardsResult = await _cardsRepository.GetAllCardsAsync();
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards: {Error}", cardsResult.Error.Description);
            return Result.Failure<List<BaseCardDto>>(cardsResult.Error);
        }

        List<BaseCardDto> cardDtos = Mapper.ToCardDTOList(cardsResult.Value);

        _logger.LogInformation("Retrieved {Count} cards.", cardDtos.Count);
        return Result.Success(cardDtos);
    }

    public async Task<Result> UpdateCardAsync()
    {
        _logger.LogInformation("Starting UpdateCardAsync.");

        var cardsResult = await GetAllCardsAsync();
        if (cardsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cards for update: {Error}", cardsResult.Error.Description);
            return Result.Failure(cardsResult.Error);
        }

        BaseCardDto chosenCard = _userInteractionService.GetCard(cardsResult.Value);
        _logger.LogInformation("User selected card ID {CardId} for update.", chosenCard.Id);

        ICardStrategy strategy = _cardStrategyFactory.GetCardStrategy(chosenCard.CardType);

        var result = await strategy.UpdateCardAsync(chosenCard.Id);
        if (result.IsSuccess)
            _logger.LogInformation("Card {CardId} updated successfully.", chosenCard.Id);
        else
            _logger.LogWarning("Failed to update card {CardId}: {Error}", chosenCard.Id, result.Error.Description);

        return result;
    }
}