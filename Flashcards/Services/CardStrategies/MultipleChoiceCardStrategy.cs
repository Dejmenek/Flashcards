using Flashcards.DataAccess.Interfaces;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Services.CardStrategies;
public class MultipleChoiceCardStrategy : ICardStrategy
{
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository? _stacksRepository;

    public MultipleChoiceCardStrategy(ICardsRepository cardsRepository, IUserInteractionService userInteractionService)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
    }

    public MultipleChoiceCardStrategy(ICardsRepository cardsRepository, IUserInteractionService userInteractionService, IStacksRepository stacksRepository)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
    }

    public async Task<Result> AddCardAsync(int stackId)
    {
        var question = _userInteractionService.GetMultipleChoiceQuestion();
        var numberOfChoices = _userInteractionService.GetNumberOfChoices();
        var choices = _userInteractionService.GetMultipleChoiceChoices(numberOfChoices);
        var answer = _userInteractionService.GetMultipleChoiceAnswers(choices);

        var addResult = await _cardsRepository.AddMultipleChoiceCardAsync(
            stackId,
            question,
            choices,
            answer
        );

        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardAsync(int cardId)
    {
        var question = _userInteractionService.GetMultipleChoiceQuestion();
        var numberOfChoices = _userInteractionService.GetNumberOfChoices();
        var choices = _userInteractionService.GetMultipleChoiceChoices(numberOfChoices);
        var answer = _userInteractionService.GetMultipleChoiceAnswers(choices);

        var updateResult = await _cardsRepository.UpdateMultipleChoiceCardAsync(
            cardId,
            question,
            choices,
            answer
        );

        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardInStackAsync(int cardId, int stackId)
    {
        var question = _userInteractionService.GetMultipleChoiceQuestion();
        var numberOfChoices = _userInteractionService.GetNumberOfChoices();
        var choices = _userInteractionService.GetMultipleChoiceChoices(numberOfChoices);
        var answer = _userInteractionService.GetMultipleChoiceAnswers(choices);

        var updateResult = await _stacksRepository.UpdateMultipleChoiceCardAsync(
            cardId,
            stackId,
            question,
            choices,
            answer
        );

        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }
}
