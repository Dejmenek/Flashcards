using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
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

    public void StartStudySessionAsync(List<FlashcardDTO> flashcards)
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

    public async Task<Result> RunStudySessionAsync(List<FlashcardDTO> studySessionFlashcards, int stackId)
    {
        if (studySessionFlashcards is [])
            return Result.Failure(FlashcardsErrors.FlashcardsNotFound);

        StartStudySessionAsync(studySessionFlashcards);
        var endResult = await EndStudySessionAsync(stackId);
        if (endResult.IsFailure) return Result.Failure(endResult.Error);

        return Result.Success();
    }

    private async Task<Result> EndStudySessionAsync(int stackId)
    {
        return await AddStudySessionAsync(stackId);
    }

    private async Task<Result> AddStudySessionAsync(int stackId)
    {
        DateTime date = DateTime.Now;
        var addResult = await _studySessionsRepository.AddStudySessionAsync(stackId, date, Score);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result<List<StudySessionDTO>>> GetAllStudySessionsAsync()
    {
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure) return Result.Failure<List<StudySessionDTO>>(hasSessionResult.Error);
        if (!hasSessionResult.Value) return Result.Success(new List<StudySessionDTO>());

        var studySessionsResult = await _studySessionsRepository.GetAllStudySessionsAsync();
        if (studySessionsResult.IsFailure) return Result.Failure<List<StudySessionDTO>>(studySessionsResult.Error);

        List<StudySessionDTO> studySessionDtos = new();
        foreach (var studySession in studySessionsResult.Value)
        {
            studySessionDtos.Add(Mapper.ToStudySessionDTO(studySession));
        }

        return Result.Success(studySessionDtos);
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionsReportAsync()
    {
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure)
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(hasSessionResult.Error);
        if (!hasSessionResult.Value) return Result.Success(Enumerable.Empty<MonthlyStudySessionsNumberData>());

        string year = _userInteractionService.GetYear();
        var reportResult = await _studySessionsRepository.GetMonthlyStudySessionReportAsync(year);
        if (reportResult.IsFailure)
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(reportResult.Error);

        return Result.Success(reportResult.Value);
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionsAverageScoreReportAsync()
    {
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure)
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(hasSessionResult.Error);
        if (!hasSessionResult.Value)
            return Result.Success(Enumerable.Empty<MonthlyStudySessionsAverageScoreData>());

        string year = _userInteractionService.GetYear();
        var reportResult = await _studySessionsRepository.GetMonthlyStudySessionAverageScoreReportAsync(year);
        if (reportResult.IsFailure)
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(reportResult.Error);

        return Result.Success(reportResult.Value);
    }
}
