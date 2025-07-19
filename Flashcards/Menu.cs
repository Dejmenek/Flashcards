using Flashcards.Controllers;
using Flashcards.Enums;
using Flashcards.Helpers;
using Flashcards.Services;
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
        DataVisualizer.ShowStacks(await _stacksController.GetAllStacksAsync());
        await _stacksController.GetStackAsync();

        while (!exitManageStacks)
        {
            Console.Clear();
            var userManageStackOption = _userInteractionService.GetManageStackOption((await _stacksController.GetCurrentStackAsync()).Name);

            switch (userManageStackOption)
            {
                case ManageStackOptions.ChangeStack:
                    await _stacksController.GetStackAsync();
                    break;

                case ManageStackOptions.ViewAllFlashcardsInStack:
                    var flashcardsInStack = await _stacksController.GetFlashcardsByStackIdAsync();
                    DataVisualizer.ShowFlashcards(flashcardsInStack);

                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.ViewAmountOfFlashcardsInStack:
                    AnsiConsole.MarkupLine($"This stack has {await _stacksController.GetFlashcardsCountInStackAsync()} flashcards.");

                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.CreateFlashcardInStack:
                    await _stacksController.AddFlashcardToStackAsync();
                    break;

                case ManageStackOptions.EditFlashcardInStack:
                    await _stacksController.UpdateFlashcardInStackAsync();
                    break;

                case ManageStackOptions.DeleteFlashcardFromStack:
                    await _stacksController.DeleteFlashcardFromStackAsync();
                    break;

                case ManageStackOptions.DeleteStack:
                    await _stacksController.DeleteStackAsync();
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.AddStack:
                    await _stacksController.AddStackAsync();
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
                await _flashcardsController.AddFlashcardAsync();

                Console.Clear();
                break;

            case ManageFlashcardsOptions.DeleteFlashcard:
                await _flashcardsController.DeleteFlashcardAsync();

                Console.Clear();
                break;

            case ManageFlashcardsOptions.ViewAllFlashcards:
                var flashcards = await _flashcardsController.GetAllFlashcardsAsync();
                DataVisualizer.ShowFlashcards(flashcards);

                _userInteractionService.GetUserInputToContinue();
                Console.Clear();
                break;

            case ManageFlashcardsOptions.EditFlashcard:
                await _flashcardsController.UpdateFlashcardAsync();

                Console.Clear();
                break;
        }
    }

    private async Task StudyAsync()
    {
        await _stacksController.GetStackAsync();

        var studySessionFlashcards = await _stacksController.GetFlashcardsByStackIdAsync();
        var currentStack = await _stacksController.GetCurrentStackAsync();
        await _studySessionsController.RunStudySessionAsync(studySessionFlashcards, currentStack.Id);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private async Task ViewStudySessionsAsync()
    {
        var studySessions = await _studySessionsController.GetAllStudySessionsAsync();
        DataVisualizer.ShowStudySessions(studySessions);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private async Task MonthlyStudySessionsReportAsync()
    {
        var monthlyStudySessionReport = await _studySessionsController.GetMonthlyStudySessionsReportAsync();
        DataVisualizer.ShowMonthlyStudySessionReport(monthlyStudySessionReport);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private async Task MonthlyStudySessionsScoreReportAsync()
    {
        var monthlyStudySessionAverageScoreReport = await _studySessionsController.GetMonthlyStudySessionsAverageScoreReportAsync();
        DataVisualizer.ShowMonthlyStudySessionAverageScoreReport(monthlyStudySessionAverageScoreReport);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }
}
