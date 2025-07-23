using Flashcards.Controllers;
using Flashcards.Enums;
using Flashcards.Helpers;
using Flashcards.Services;
using Flashcards.Utils;
using Spectre.Console;

namespace Flashcards;
public class Menu
{
    private readonly UserInteractionService _userInteractionService;
    private readonly StacksController _stacksController;
    private readonly FlashcardsController _flashcardsController;
    private readonly StudySessionsController _studySessionsController;

    public Menu(UserInteractionService userInteractionService, StacksController stacksController, FlashcardsController flashcardsController, StudySessionsController studySessionsController)
    {
        _userInteractionService = userInteractionService;
        _stacksController = stacksController;
        _flashcardsController = flashcardsController;
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

                case MenuOptions.ManageFlashcards:
                    await ManageFlashcardsAsync();
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

                case ManageStackOptions.ViewAllFlashcardsInStack:
                    var flashcardsInStackResult = await _stacksController.GetFlashcardsByStackIdAsync();
                    if (flashcardsInStackResult.IsFailure)
                    {
                        ShowError(flashcardsInStackResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                        break;
                    }
                    DataVisualizer.ShowFlashcards(flashcardsInStackResult.Value);
                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.ViewAmountOfFlashcardsInStack:
                    var countResult = await _stacksController.GetFlashcardsCountInStackAsync();
                    if (countResult.IsFailure)
                    {
                        ShowError(countResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                        break;
                    }
                    AnsiConsole.MarkupLine($"This stack has {countResult.Value} flashcards.");
                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.CreateFlashcardInStack:
                    var addFlashcardResult = await _stacksController.AddFlashcardToStackAsync();
                    if (addFlashcardResult.IsFailure)
                    {
                        ShowError(addFlashcardResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    break;

                case ManageStackOptions.EditFlashcardInStack:
                    var updateFlashcardResult = await _stacksController.UpdateFlashcardInStackAsync();
                    if (updateFlashcardResult.IsFailure)
                    {
                        ShowError(updateFlashcardResult.Error);
                        _userInteractionService.GetUserInputToContinue();
                    }
                    break;

                case ManageStackOptions.DeleteFlashcardFromStack:
                    var deleteFlashcardResult = await _stacksController.DeleteFlashcardFromStackAsync();
                    if (deleteFlashcardResult.IsFailure)
                    {
                        ShowError(deleteFlashcardResult.Error);
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

    private async Task ManageFlashcardsAsync()
    {
        var userManageFlashcardsOption = _userInteractionService.GetManageFlashcardsOption();

        switch (userManageFlashcardsOption)
        {
            case ManageFlashcardsOptions.AddFlashcard:
                var addResult = await _flashcardsController.AddFlashcardAsync();
                if (addResult.IsFailure)
                {
                    ShowError(addResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                }
                Console.Clear();
                break;

            case ManageFlashcardsOptions.DeleteFlashcard:
                var deleteResult = await _flashcardsController.DeleteFlashcardAsync();
                if (deleteResult.IsFailure)
                {
                    ShowError(deleteResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                }
                Console.Clear();
                break;

            case ManageFlashcardsOptions.ViewAllFlashcards:
                var flashcardsResult = await _flashcardsController.GetAllFlashcardsAsync();
                if (flashcardsResult.IsFailure)
                {
                    ShowError(flashcardsResult.Error);
                    _userInteractionService.GetUserInputToContinue();
                    Console.Clear();
                    break;
                }
                DataVisualizer.ShowFlashcards(flashcardsResult.Value);
                _userInteractionService.GetUserInputToContinue();
                Console.Clear();
                break;

            case ManageFlashcardsOptions.EditFlashcard:
                var updateResult = await _flashcardsController.UpdateFlashcardAsync();
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
        var stackResult = await _stacksController.GetStackAsync();
        if (stackResult.IsFailure)
        {
            ShowError(stackResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }

        var flashcardsResult = await _stacksController.GetFlashcardsByStackIdAsync();
        if (flashcardsResult.IsFailure)
        {
            ShowError(flashcardsResult.Error);
            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
            return;
        }

        var currentStack = _stacksController.GetCurrentStack();
        var studySessionResult = await _studySessionsController.RunStudySessionAsync(flashcardsResult.Value, currentStack.Id);
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
            case ErrorType.Failure:
            default:
                AnsiConsole.MarkupLine($"[red]Error: {error.Description}[/]");
                break;
        }
    }
}
