﻿using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Spectre.Console;

namespace Flashcards.Services;

public class StudySessionsService : IStudySessionsService
{
    private readonly IStudySessionsRepository _studySessionsRepository;
    private readonly UserInteractionService _userInteractionService;
    public int Score { get; private set; }

    public StudySessionsService(IStudySessionsRepository studySessionsRepository, UserInteractionService userInteractionService)
    {
        _studySessionsRepository = studySessionsRepository;
        _userInteractionService = userInteractionService;
    }

    public void StartStudySession(List<FlashcardDTO> flashcards)
    {
        Score = 0;
        foreach (FlashcardDTO flashcard in flashcards)
        {
            DataVisualizer.ShowFlashcardFront(flashcard);
            string userAnswer = _userInteractionService.GetAnswer();

            if (userAnswer == flashcard.Back)
            {
                AnsiConsole.MarkupLine("Your answer is correct!");
                Score++;
            }
            else
            {
                AnsiConsole.MarkupLine("Your answwer was wrong.");
                AnsiConsole.MarkupLine($"The correct answer was {flashcard.Back}.");
            }

            _userInteractionService.GetUserInputToContinue();
            Console.Clear();
        }

        AnsiConsole.MarkupLine($"You got {Score} out of {flashcards.Count}");
        _userInteractionService.GetUserInputToContinue();
        Console.Clear();
    }

    public void RunStudySession(List<FlashcardDTO> studySessionFlashcards, int stackId)
    {
        if (studySessionFlashcards is [])
        {
            AnsiConsole.MarkupLine("No flashcards found to study.");
            return;
        }

        StartStudySession(studySessionFlashcards);
        EndStudySession(stackId);
    }

    private void EndStudySession(int stackId)
    {
        AddStudySession(stackId);
    }

    private void AddStudySession(int stackId)
    {
        DateTime date = DateTime.Now;

        _studySessionsRepository.AddStudySession(stackId, date, Score);
    }

    public List<StudySessionDTO> GetAllStudySessions()
    {
        if (!_studySessionsRepository.HasStudySession())
        {
            return [];
        }

        List<StudySessionDTO> studySessionDtos = new List<StudySessionDTO>();
        var studySessions = _studySessionsRepository.GetAllStudySessions();

        foreach (var studySession in studySessions)
        {
            studySessionDtos.Add(Mapper.ToStudySessionDTO(studySession));
        }

        return studySessionDtos;
    }

    public IEnumerable<MonthlyStudySessionsNumberData> GetMonthlyStudySessionsReport()
    {

        if (!_studySessionsRepository.HasStudySession())
        {
            return Enumerable.Empty<MonthlyStudySessionsNumberData>();
        }

        string year = _userInteractionService.GetYear();

        return _studySessionsRepository.GetMonthlyStudySessionReport(year);
    }

    public IEnumerable<MonthlyStudySessionsAverageScoreData> GetMonthlyStudySessionsAverageScoreReport()
    {
        if (!_studySessionsRepository.HasStudySession())
        {
            return Enumerable.Empty<MonthlyStudySessionsAverageScoreData>();
        }

        string year = _userInteractionService.GetYear();

        return _studySessionsRepository.GetMonthlyStudySessionAverageScoreReport(year);
    }
}
