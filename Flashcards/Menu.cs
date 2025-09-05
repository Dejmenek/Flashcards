using Flashcards.Controllers;
using Flashcards.Enums;
using Flashcards.Helpers;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Spectre.Console;

namespace Flashcards;
public class Menu
{
    private readonly IUserInteractionService _userInteractionService;
    private readonly StacksController _stacksController;
    private readonly CardsController _cardsController;
    private readonly StudySessionsController _studySessionsController;

    public Menu(IUserInteractionService userInteractionService, StacksController stacksController, CardsController cardsController, StudySessionsController studySessionsController)
    {
        _userInteractionService = userInteractionService;
        _stacksController = stacksController;
        _cardsController = cardsController;
        _studySessionsController = studySessionsController;
    }

    public async Task RunAsync()
    {
        bool exitMenuOptions = false;

        while (!exitMenuOptions)
        {
            var userOption = _userInteractionService.GetMenuOption();

            switch (userOption)
            {
                case MenuOptions.Exit:
                    exitMenuOptions = true;
                    break;

                case MenuOptions.ManageStacks:
                    await ManageStacksAsync();
                    break;

                case MenuOptions.ManageCards:
                    await ManageCardsAsync();
                    break;

                case MenuOptions.Study:
                    await StudyAsync();
                    break;

                case MenuOptions.ViewStudySessions:
                    await ViewStudySessionsAsync();
                    break;

                case MenuOptions.MonthlyStudySessionsReport:
                    await MonthlyStudySessionsReportAsync();
                    break;

                case MenuOptions.MonthlyStudySessionsScoreReport:
                    await MonthlyStudySessionsScoreReportAsync();
                    break;
            }
        }
    }

    private async Task ManageStacksAsync()
    {
        bool exitManageStacks = false;
        var stacksResult = await _stacksController.GetAllStacksAsync();
        if (stacksResult.IsFailure)
        {
            ShowError(stacksResult.Error);
            return;
        }
        DataVisualizer.ShowStacks(stacksResult.Value);

        var stackResult = await _stacksController.GetStackAsync();
        if (stackResult.IsFailure)
        {
            ShowError(stackResult.Error);
            return;
        }

        while (!exitManageStacks)
        {
            Console.Clear();
            var currentStack = _stacksController.GetCurrentStack();
            var userManageStackOption = _userInteractionService.GetManageStackOption(currentStack.Name);

            switch (userManageStackOption)
            {
                case ManageStackOptions.ChangeStack:
                    var changeStackResult = await _stacksController.GetStackAsync();
                    if (changeStackResult.IsFailure)
                    {
                        ShowError(changeStackResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    break;

                case ManageStackOptions.ViewCardsInStack:
                    var cardsInStackResult = await _stacksController.GetCardsByStackIdAsync();
                    if (cardsInStackResult.IsFailure)
                    {
                        ShowError(cardsInStackResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                        break;
                    }
                    DataVisualizer.ShowCards(cardsInStackResult.Value);
                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.ViewAmountOfCardsInStack:
                    var countResult = await _stacksController.GetCardsCountInStackAsync();
                    if (countResult.IsFailure)
                    {
                        ShowError(countResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                        break;
                    }
                    AnsiConsole.MarkupLine($"This stack has {countResult.Value} cards.");
                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.CreateCardInStack:
                    var addCardResult = await _stacksController.AddCardToStackAsync();
                    if (addCardResult.IsFailure)
                    {
                        ShowError(addCardResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    break;

                case ManageStackOptions.EditCardInStack:
                    var updateCardResult = await _stacksController.UpdateCardInStackAsync();
                    if (updateCardResult.IsFailure)
                    {
                        ShowError(updateCardResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    break;

                case ManageStackOptions.DeleteCardFromStack:
                    var deleteCardResult = await _stacksController.DeleteCardFromStackAsync();
                    if (deleteCardResult.IsFailure)
                    {
                        ShowError(deleteCardResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    break;

                case ManageStackOptions.DeleteStack:
                    var deleteStackResult = await _stacksController.DeleteStackAsync();
                    if (deleteStackResult.IsFailure)
                    {
                        ShowError(deleteStackResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.AddStack:
                    var addStackResult = await _stacksController.AddStackAsync();
                    if (addStackResult.IsFailure)
                    {
                        ShowError(addStackResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.Exit:
                    exitManageStacks = true;
                    break;
            }
        }
    }

    private async Task ManageCardsAsync()
    {
        var userManageCardsOption = _userInteractionService.GetManageCardsOption();

        switch (userManageCardsOption)
        {
            case ManageCardsOptions.AddCcard:
                var addResult = await _cardsController.AddCardAsync();
                if (addResult.IsFailure)
                {
                    ShowError(addResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                }
                Console.Clear();
                break;

            case ManageCardsOptions.DeleteCard:
                var deleteResult = await _cardsController.DeleteCardAsync();
                if (deleteResult.IsFailure)
                {
                    ShowError(deleteResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                }
                Console.Clear();
                break;

            case ManageCardsOptions.ViewAllCards:
                var cardsResult = await _cardsController.GetAllCardsAsync();
                if (cardsResult.IsFailure)
                {
                    ShowError(cardsResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                    Console.Clear();
                    break;
                }
                DataVisualizer.ShowCards(cardsResult.Value);
                _userInteractionService.GetUserInputToContinue();
                Console.Clear();
                break;

            case ManageCardsOptions.EditCard:
                var updateResult = await _cardsController.UpdateCardAsync();
                if (updateResult.IsFailure)
                {
                    ShowError(updateResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                }
                Console.Clear();
                break;
        }
    }

    private async Task StudyAsync()
    {
        var stackSummaries = await _stacksController.GetAllStackSummariesAsync();
        DataVisualizer.ShowStacksSummary(stackSummaries.Value);

        var stackResult = await _stacksController.GetStackToStudyAsync();
        if (stackResult.IsFailure)
        {
            ShowError(stackResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }

        var cardsResult = await _stacksController.GetCardsToStudyByStackIdAsync();
        if (cardsResult.IsFailure)
        {
            ShowError(cardsResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }

        var currentStack = _stacksController.GetCurrentStack();
        var studySessionResult = await _studySessionsController.RunStudySessionAsync(cardsResult.Value, currentStack.Id);
        if (studySessionResult.IsFailure)
        {
            ShowError(studySessionResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private async Task ViewStudySessionsAsync()
    {
        var studySessionsResult = await _studySessionsController.GetAllStudySessionsAsync();
        if (studySessionsResult.IsFailure)
        {
            ShowError(studySessionsResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }
        DataVisualizer.ShowStudySessions(studySessionsResult.Value);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private async Task MonthlyStudySessionsReportAsync()
    {
        var monthlyReportResult = await _studySessionsController.GetMonthlyStudySessionsReportAsync();
        if (monthlyReportResult.IsFailure)
        {
            ShowError(monthlyReportResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }
        DataVisualizer.ShowMonthlyStudySessionReport(monthlyReportResult.Value);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private async Task MonthlyStudySessionsScoreReportAsync()
    {
        var scoreReportResult = await _studySessionsController.GetMonthlyStudySessionsAverageScoreReportAsync();
        if (scoreReportResult.IsFailure)
        {
            ShowError(scoreReportResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }
        DataVisualizer.ShowMonthlyStudySessionAverageScoreReport(scoreReportResult.Value);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private static void ShowError(Error error)
    {
        switch (error.Type)
        {
            case ErrorType.NotFound:
                AnsiConsole.MarkupLine($"[yellow]Not Found: {error.Description}[/]");
                break;
            case ErrorType.Validation:
                AnsiConsole.MarkupLine($"[yellow]Validation Error: {error.Description}[/]");
                break;
            case ErrorType.Problem:
                AnsiConsole.MarkupLine($"[red]Problem: {error.Description}[/]");
                break;
            case ErrorType.Conflict:
                AnsiConsole.MarkupLine($"[red]Conflict: {error.Description}[/]");
                break;
            default:
                AnsiConsole.MarkupLine($"[red]Error: {error.Description}[/]");
                break;
        }
    }
}