﻿using Flashcards.Controllers;
using Flashcards.DataAccess;
using Flashcards.DataAccess.Repositories;
using Flashcards.Enums;
using Flashcards.Helpers;
using Flashcards.Services;
using Spectre.Console;

internal class Program
{
    private static void Main()
    {
        DataContext.CreateDatabase();

        bool exitMenuOptions = false;
        var userInteractionService = new UserInteractionService();
        var flashcardsRepository = new FlashcardsRepository();
        var stacksRepository = new StacksRepository();
        var flashcardsService = new FlashcardsService(flashcardsRepository, userInteractionService, stacksRepository);
        var flashcardsController = new FlashcardsController(flashcardsService);
        var stacksService = new StacksService(stacksRepository, userInteractionService, flashcardsRepository);
        var stacksController = new StacksController(stacksService);
        var studySessionsRepository = new StudySessionsRepository();
        var studySessionsService = new StudySessionsService(studySessionsRepository, userInteractionService);
        var studySessionsController = new StudySessionsController(studySessionsService);

        while (!exitMenuOptions)
        {
            var userOption = userInteractionService.GetMenuOption();

            switch (userOption)
            {
                case MenuOptions.Exit:
                    exitMenuOptions = true;
                    break;

                case MenuOptions.ManageStacks:
                    ManageStacks(stacksController, userInteractionService);
                    break;

                case MenuOptions.ManageFlashcards:
                    ManageFlashcards(flashcardsController, userInteractionService);
                    break;

                case MenuOptions.Study:
                    Study(stacksController, studySessionsController, userInteractionService);
                    break;

                case MenuOptions.ViewStudySessions:
                    ViewStudySessions(studySessionsController, userInteractionService);
                    break;

                case MenuOptions.MonthlyStudySessionsReport:
                    MonthlyStudySessionsReport(studySessionsController, userInteractionService);
                    break;

                case MenuOptions.MonthlyStudySessionsScoreReport:
                    MonthlyStudySessionsScoreReport(studySessionsController, userInteractionService);
                    break;
            }
        }
    }

    private static void ManageStacks(StacksController stacksController, UserInteractionService userInteractionService)
    {
        bool exitManageStacks = false;
        DataVisualizer.ShowStacks(stacksController.GetAllStacks());
        stacksController.GetStack();

        while (!exitManageStacks)
        {
            Console.Clear();
            var userManageStackOption = userInteractionService.GetManageStackOption(stacksController.GetCurrentStack().Name);

            switch (userManageStackOption)
            {
                case ManageStackOptions.ChangeStack:
                    stacksController.GetStack();
                    break;

                case ManageStackOptions.ViewAllFlashcardsInStack:
                    var flashcardsInStack = stacksController.GetFlashcardsByStackId();
                    DataVisualizer.ShowFlashcards(flashcardsInStack);

                    userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.ViewAmountOfFlashcardsInStack:
                    AnsiConsole.MarkupLine($"This stack has {stacksController.GetFlashcardsCountInStack()} flashcards.");

                    userInteractionService.GetUserInputToContinue();
                    break;

                case ManageStackOptions.CreateFlashcardInStack:
                    stacksController.AddFlashcardToStack();
                    break;

                case ManageStackOptions.EditFlashcardInStack:
                    stacksController.UpdateFlashcardInStack();
                    break;

                case ManageStackOptions.DeleteFlashcardFromStack:
                    stacksController.DeleteFlashcardFromStack();
                    break;

                case ManageStackOptions.DeleteStack:
                    stacksController.DeleteStack();
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.AddStack:
                    stacksController.AddStack();
                    exitManageStacks = true;
                    break;

                case ManageStackOptions.Exit:
                    exitManageStacks = true;
                    break;
            }
        }
    }

    private static void ManageFlashcards(FlashcardsController flashcardsController, UserInteractionService userInteractionService)
    {
        var userManageFlashcardsOption = userInteractionService.GetManageFlashcardsOption();

        switch (userManageFlashcardsOption)
        {
            case ManageFlashcardsOptions.AddFlashcard:
                flashcardsController.AddFlashcard();

                Console.Clear();
                break;

            case ManageFlashcardsOptions.DeleteFlashcard:
                flashcardsController.DeleteFlashcard();

                Console.Clear();
                break;

            case ManageFlashcardsOptions.ViewAllFlashcards:
                var flashcards = flashcardsController.GetAllFlashcards();
                DataVisualizer.ShowFlashcards(flashcards);

                userInteractionService.GetUserInputToContinue();
                Console.Clear();
                break;

            case ManageFlashcardsOptions.EditFlashcard:
                flashcardsController.UpdateFlashcard();

                Console.Clear();
                break;
        }
    }

    private static void Study(StacksController stacksController, StudySessionsController studySessionsController, UserInteractionService userInteractionService)
    {
        stacksController.GetStack();

        var studySessionFlashcards = stacksController.GetFlashcardsByStackId();
        studySessionsController.RunStudySession(studySessionFlashcards, stacksController.GetCurrentStack().Id);

        userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private static void ViewStudySessions(StudySessionsController studySessionsController, UserInteractionService userInteractionService)
    {
        var studySessions = studySessionsController.GetAllStudySessions();
        DataVisualizer.ShowStudySessions(studySessions);

        userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private static void MonthlyStudySessionsReport(StudySessionsController studySessionsController, UserInteractionService userInteractionService)
    {
        var monthlyStudySessionReport = studySessionsController.GetMonthlyStudySessionsReport();
        DataVisualizer.ShowMonthlyStudySessionReport(monthlyStudySessionReport);

        userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    private static void MonthlyStudySessionsScoreReport(StudySessionsController studySessionsController, UserInteractionService userInteractionService)
    {
        var monthlyStudySessionAverageScoreReport = studySessionsController.GetMonthlyStudySessionsAverageScoreReport();
        DataVisualizer.ShowMonthlyStudySessionAverageScoreReport(monthlyStudySessionAverageScoreReport);

        userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }
}