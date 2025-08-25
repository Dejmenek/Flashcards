using System.Text.RegularExpressions;

using Flashcards.DataAccess.Interfaces;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Services.CardStrategies;
public class ClozeCardStrategy : ICardStrategy
{
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository? _stacksRepository;

    public ClozeCardStrategy(ICardsRepository cardsRepository, IUserInteractionService userInteractionService)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
    }

    public ClozeCardStrategy(ICardsRepository cardsRepository, IUserInteractionService userInteractionService, IStacksRepository stacksRepository)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
    }
    public async Task<Result> AddCardAsync(int stackId)
    {
        string clozeText = _userInteractionService.GetClozeDeletionText();
        List<string> wordsToHide = _userInteractionService.GetClozeDeletionWords(clozeText);

        int counter = 1;

        string clozeFormattedText = Regex.Replace(clozeText, @"\b(\w+)(\W*)?", match =>
        {
            string word = match.Groups[1].Value;
            string trailing = match.Groups[2].Value;

            if (wordsToHide.Contains(word))
            {
                return $"{{{{c{counter++}::{word}}}}}{trailing}";
            }

            return match.Value;
        });

        var addResult = await _cardsRepository.AddClozeCardAsync(stackId, clozeFormattedText);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardAsync(int cardId)
    {
        string clozeText = _userInteractionService.GetClozeDeletionText();
        List<string> wordsToHide = _userInteractionService.GetClozeDeletionWords(clozeText);

        int counter = 1;

        string clozeFormattedText = Regex.Replace(clozeText, @"\b(\w+)(\W*)?", match =>
        {
            string word = match.Groups[1].Value;
            string trailing = match.Groups[2].Value;

            if (wordsToHide.Contains(word))
            {
                return $"{{{{c{counter++}::{word}}}}}{trailing}";
            }

            return match.Value;
        });

        var updateResult = await _cardsRepository.UpdateClozeCardAsync(cardId, clozeFormattedText);
        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateCardInStackAsync(int cardId, int stackId)
    {
        string clozeText = _userInteractionService.GetClozeDeletionText();
        List<string> wordsToHide = _userInteractionService.GetClozeDeletionWords(clozeText);

        int counter = 1;

        string clozeFormattedText = Regex.Replace(clozeText, @"\b(\w+)(\W*)?", match =>
        {
            string word = match.Groups[1].Value;
            string trailing = match.Groups[2].Value;

            if (wordsToHide.Contains(word))
            {
                return $"{{{{c{counter++}::{word}}}}}{trailing}";
            }

            return match.Value;
        });

        var updateResult = await _stacksRepository.UpdateClozeCardInStackAsync(cardId, stackId, clozeFormattedText);
        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }
}