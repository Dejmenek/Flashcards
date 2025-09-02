using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace Flashcards.Services.CardStrategies;
public class MultipleChoiceCardStudyStrategy : IStudyCardStrategy
{
    private readonly IUserInteractionService _userInteractionService;
    private readonly ILogger _logger;

    public MultipleChoiceCardStudyStrategy(IUserInteractionService userInteractionService, ILogger logger)
    {
        _userInteractionService = userInteractionService;
        _logger = logger;
    }

    public bool Study(BaseCardDto card)
    {
        var multipleChoiceCard = (MultipleChoiceCardDto)card;
        DataVisualizer.ShowMultipleChoiceCard(multipleChoiceCard.Question);
        List<string> userAnswers = _userInteractionService.GetMultipleChoiceAnswers(multipleChoiceCard.Choices);

        bool correct = IsCorrectMultipleChoiceCardAnswer(userAnswers, multipleChoiceCard.Answer);

        if (correct)
        {
            AnsiConsole.MarkupLine("Your answer is correct!");
            _logger.LogInformation("Correct answer for card ID {CardId}.", multipleChoiceCard.Id);
        }
        else
        {
            AnsiConsole.MarkupLine("Your answer was wrong.");
            AnsiConsole.MarkupLine($"The correct answer was {string.Join(',', multipleChoiceCard.Answer)}.");
            _logger.LogInformation("Incorrect answer for card ID {CardId}.", multipleChoiceCard.Id);
        }

        return correct;
    }

    private static bool IsCorrectMultipleChoiceCardAnswer(List<string> userAnswers, List<string> correctAnswers)
    {
        if (userAnswers.Count != correctAnswers.Count)
            return false;

        var userAnswersSet = new HashSet<string>(userAnswers);
        var correctAnswersSet = new HashSet<string>(correctAnswers);

        return userAnswersSet.SetEquals(correctAnswersSet);
    }
}