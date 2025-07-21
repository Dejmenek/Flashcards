using Flashcards.DataAccess.Interfaces;
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

    public async Task RunStudySessionAsync(List<FlashcardDTO> studySessionFlashcards, int stackId)
    {
        if (studySessionFlashcards is [])
        {
            AnsiConsole.MarkupLine("No flashcards found to study.");
            return;
        }

        StartStudySessionAsync(studySessionFlashcards);
        await EndStudySessionAsync(stackId);
    }

    private async Task EndStudySessionAsync(int stackId)
    {
        await AddStudySessionAsync(stackId);
    }

    private async Task AddStudySessionAsync(int stackId)
    {
        DateTime date = DateTime.Now;
        await _studySessionsRepository.AddStudySessionAsync(stackId, date, Score);
    }

    public async Task<List<StudySessionDTO>> GetAllStudySessionsAsync()
    {
        if (!await _studySessionsRepository.HasStudySessionAsync())
        {
            return [];
        }

        List<StudySessionDTO> studySessionDtos = new List<StudySessionDTO>();
        var studySessions = await _studySessionsRepository.GetAllStudySessionsAsync();

        foreach (var studySession in studySessions)
        {
            studySessionDtos.Add(Mapper.ToStudySessionDTO(studySession));
        }

        return studySessionDtos;
    }

    public async Task<IEnumerable<MonthlyStudySessionsNumberData>> GetMonthlyStudySessionsReportAsync()
    {
        if (!await _studySessionsRepository.HasStudySessionAsync())
        {
            return Enumerable.Empty<MonthlyStudySessionsNumberData>();
        }

        string year = _userInteractionService.GetYear();

        return await _studySessionsRepository.GetMonthlyStudySessionReportAsync(year);
    }

    public async Task<IEnumerable<MonthlyStudySessionsAverageScoreData>> GetMonthlyStudySessionsAverageScoreReportAsync()
    {
        if (!await _studySessionsRepository.HasStudySessionAsync())
        {
            return Enumerable.Empty<MonthlyStudySessionsAverageScoreData>();
        }

        string year = _userInteractionService.GetYear();

        return await _studySessionsRepository.GetMonthlyStudySessionAverageScoreReportAsync(year);
    }
}
