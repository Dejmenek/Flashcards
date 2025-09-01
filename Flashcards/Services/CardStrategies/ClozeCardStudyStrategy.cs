using System.Text.RegularExpressions;

using Flashcards.Models;
using Flashcards.Services.Interfaces;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace Flashcards.Services.CardStrategies;
public class ClozeCardStudyStrategy : IStudyCardStrategy
{
    private readonly IUserInteractionService _userInteractionService;
    private readonly ILogger _logger;

    public ClozeCardStudyStrategy(IUserInteractionService userInteractionService, ILogger logger)
    {
        _userInteractionService = userInteractionService;
        _logger = logger;
    }

    public bool Study(BaseCardDto card)
    {
        var clozeCard = (ClozeCardDto)card;

        var clozePattern = @"\{\{c\d+::(.*?)\}\}";
        var matches = Regex.Matches(clozeCard.ClozeText, clozePattern);

        string displayText = clozeCard.ClozeText;
        foreach (var clozeTag in from Match match in matches
                                 let clozeTag = match.Value
                                 select clozeTag)
        {
            displayText = displayText.Replace(clozeTag, "[bold underline]_____[/]");
        }

        AnsiConsole.MarkupLine("Fill in the blanks for the following sentence:");
        AnsiConsole.MarkupLine(displayText);

        List<string> correctAnswers = new();
        List<string> userClozeCardAnswers = new();
        int clozeIndex = 1;

        foreach (Match match in matches)
        {
            string hiddenText = match.Groups[1].Value;
            correctAnswers.Add(hiddenText);

            string prompt = $"Fill in the blank for cloze {clozeIndex}: ";
            AnsiConsole.MarkupLine(prompt);
            string userClozeCardAnswer = _userInteractionService.GetAnswer();
            userClozeCardAnswers.Add(userClozeCardAnswer);
            clozeIndex++;
        }

        string userFilledText = clozeCard.ClozeText;
        clozeIndex = 0;
        foreach (var (clozeTag, replacement) in from Match match in matches
                                                let clozeTag = match.Value
                                                let replacement = $"[bold yellow]{userClozeCardAnswers[clozeIndex]}[/]"
                                                select (clozeTag, replacement))
        {
            userFilledText = userFilledText.Replace(clozeTag, replacement);
            clozeIndex++;
        }

        AnsiConsole.MarkupLine("Your completed sentence:");
        AnsiConsole.MarkupLine(userFilledText);

        string correctFilledText = clozeCard.ClozeText;
        clozeIndex = 0;
        foreach (var (clozeTag, replacement) in from Match match in matches
                                                let clozeTag = match.Value
                                                let replacement = $"[bold green]{correctAnswers[clozeIndex]}[/]"
                                                select (clozeTag, replacement))
        {
            correctFilledText = correctFilledText.Replace(clozeTag, replacement);
            clozeIndex++;
        }

        AnsiConsole.MarkupLine("Correct sentence:");
        AnsiConsole.MarkupLine(correctFilledText);

        bool allCorrect = true;
        for (int i = 0; i < correctAnswers.Count; i++)
        {
            if (!string.Equals(userClozeCardAnswers[i], correctAnswers[i], StringComparison.OrdinalIgnoreCase))
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            AnsiConsole.MarkupLine("Your answers are correct!");
            _logger.LogInformation("Correct cloze answers for card ID {CardId}.", clozeCard.Id);
        }
        else
        {
            AnsiConsole.MarkupLine("Your answers were wrong.");
            _logger.LogInformation("Incorrect cloze answers for card ID {CardId}.", clozeCard.Id);
        }

        return allCorrect;
    }
}
