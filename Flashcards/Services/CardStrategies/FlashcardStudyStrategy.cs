using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace Flashcards.Services.CardStrategies;
public class FlashcardStudyStrategy : IStudyCardStrategy
{
    private readonly IUserInteractionService _userInteractionService;
    private readonly ILogger _logger;
    public FlashcardStudyStrategy(IUserInteractionService userInteractionService, ILogger logger)
    {
        _userInteractionService = userInteractionService;
        _logger = logger;
    }

    public bool Study(BaseCardDto card)
    {
        var flashcard = (FlashcardDto)card;

        DataVisualizer.ShowFlashcardFront(flashcard);
        string userAnswer = _userInteractionService.GetAnswer();

        if (userAnswer == flashcard.Back)
        {
            AnsiConsole.MarkupLine("Your answer is correct!");
            _logger.LogInformation("Correct answer for card ID {CardId}.", flashcard.Id);
            return true;
        }
        else
        {
            AnsiConsole.MarkupLine("Your answer was wrong.");
            AnsiConsole.MarkupLine($"The correct answer was {flashcard.Back}.");
            _logger.LogInformation("Incorrect answer for card ID {CardId}.", flashcard.Id);
            return false;
        }
    }
}