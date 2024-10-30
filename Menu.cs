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

    public void Run()
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
                    ManageStacks();
                    break;

                case MenuOptions.ManageFlashcards:
                    ManageFlashcards();
                    break;

                case MenuOptions.Study:
                    Study();
                    break;

                case MenuOptions.ViewStudySessions:
                    ViewStudySessions();
                    break;

                case MenuOptions.MonthlyStudySessionsReport:
                    MonthlyStudySessionsReport();
                    break;

                case MenuOptions.MonthlyStudySessionsScoreReport:
                    MonthlyStudySessionsScoreReport();
                    break;
            }
        }
    }

    private void ManageStacks()
    {
        bool exitManageStacks = false;
        DataVisualizer.ShowStacks(_stacksController.GetAllStacks());
        _stacksController.GetStack();

        while (!exitManageStacks)
        {
            Console.Clear();
            var userManageStackOption = _userInteractionService.GetManageStackOption(_stacksController.GetCurrentStack().Name);

            switch (userManageStackOption)
            {
                case ManageStackOptions.ChangeStack:
                    _stacksController.GetStack();
                    break;

                case ManageStackOptions.ViewAllFlashcardsInStack:
                    var flashcardsInStack = _stacksController.GetFlashcardsByStackId();
                    DataVisualizer.ShowFlashcards(flashcardsInStack);

                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.ViewAmountOfFlashcardsInStack:
                    AnsiConsole.MarkupLine($"This stack has {_stacksController.GetFlashcardsCountInStack()} flashcards.");

                    _userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.CreateFlashcardInStack:
                    _stacksController.AddFlashcardToStack();
                    break;

                case ManageStackOptions.EditFlashcardInStack:
                    _stacksController.UpdateFlashcardInStack();
                    break;

                case ManageStackOptions.DeleteFlashcardFromStack:
                    _stacksController.DeleteFlashcardFromStack();
                    break;

                case ManageStackOptions.DeleteStack:
                    _stacksController.DeleteStack();
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.AddStack:
                    _stacksController.AddStack();
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.Exit:
                    exitManageStacks = true;
                    break;
            }
        }
    }

    private void ManageFlashcards()
    {
        var userManageFlashcardsOption = _userInteractionService.GetManageFlashcardsOption();

        switch (userManageFlashcardsOption)
        {
            case ManageFlashcardsOptions.AddFlashcard:
                _flashcardsController.AddFlashcard();

                Console.Clear();
                break;

            case ManageFlashcardsOptions.DeleteFlashcard:
                _flashcardsController.DeleteFlashcard();

                Console.Clear();
                break;

            case ManageFlashcardsOptions.ViewAllFlashcards:
                var flashcards = _flashcardsController.GetAllFlashcards();
                DataVisualizer.ShowFlashcards(flashcards);

                _userInteractionService.GetUserInputToContinue();
                Console.Clear();
                break;

            case ManageFlashcardsOptions.EditFlashcard:
                _flashcardsController.UpdateFlashcard();

                Console.Clear();
                break;
        }
    }

    private void Study()
    {
        _stacksController.GetStack();

        var studySessionFlashcards = _stacksController.GetFlashcardsByStackId();
        _studySessionsController.RunStudySession(studySessionFlashcards, _stacksController.GetCurrentStack().Id);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private void ViewStudySessions()
    {
        var studySessions = _studySessionsController.GetAllStudySessions();
        DataVisualizer.ShowStudySessions(studySessions);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private void MonthlyStudySessionsReport()
    {
        var monthlyStudySessionReport = _studySessionsController.GetMonthlyStudySessionsReport();
        DataVisualizer.ShowMonthlyStudySessionReport(monthlyStudySessionReport);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private void MonthlyStudySessionsScoreReport()
    {
        var monthlyStudySessionAverageScoreReport = _studySessionsController.GetMonthlyStudySessionsAverageScoreReport();
        DataVisualizer.ShowMonthlyStudySessionAverageScoreReport(monthlyStudySessionAverageScoreReport);

        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }
}
