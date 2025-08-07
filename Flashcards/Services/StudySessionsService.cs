using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Flashcards.Services;

public class StudySessionsService : IStudySessionsService
{
    private readonly IStudySessionsRepository _studySessionsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IConsoleService _consoleService;
    private readonly ILogger<StudySessionsService> _logger;
    public int Score { get; private set; }

    public StudySessionsService(
        IStudySessionsRepository studySessionsRepository,
        IUserInteractionService userInteractionService,
        IConsoleService consoleService,
        ILogger<StudySessionsService> logger)
    {
        _studySessionsRepository = studySessionsRepository;
        _userInteractionService = userInteractionService;
        _consoleService = consoleService;
        _logger = logger;
    }

    public void StartStudySessionAsync(List<BaseCardDTO> cards)
    {
        _logger.LogInformation("Starting study session with {Count} cards.", cards.Count);
        Score = 0;
        foreach (BaseCardDTO card in cards)
        {
            switch (card)
            {
                case FlashcardDTO flashcard:
                    DataVisualizer.ShowFlashcardFront(flashcard);
                    string userAnswer = _userInteractionService.GetAnswer();

                    if (userAnswer == flashcard.Back)
                    {
                        AnsiConsole.MarkupLine("Your answer is correct!");
                        Score++;
                        _logger.LogInformation("Correct answer for card ID {CardId}.", flashcard.Id);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("Your answwer was wrong.");
                        AnsiConsole.MarkupLine($"The correct answer was {flashcard.Back}.");
                        _logger.LogInformation("Incorrect answer for card ID {CardId}.", flashcard.Id);
                    }
                    break;
                case MultipleChoiceCardDTO multipleChoiceCard:
                    DataVisualizer.ShowMultipleChoiceCard(multipleChoiceCard.Question);
                    List<string> userAnswers = _userInteractionService.GetMultipleChoiceAnswers(multipleChoiceCard.Choices);
                    if (IsCorrectMultipleChoiceCardAnswer(userAnswers, multipleChoiceCard.Answer))
                    {
                        AnsiConsole.MarkupLine("Your answer is correct!");
                        Score++;
                        _logger.LogInformation("Correct answer for card ID {CardId}.", multipleChoiceCard.Id);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("Your answer was wrong.");
                        AnsiConsole.MarkupLine($"The correct answer was {string.Join(',', multipleChoiceCard.Answer)}.");
                        _logger.LogInformation("Incorrect answer for card ID {CardId}.", multipleChoiceCard.Id);
                    }
                    break;
            }

            _userInteractionService.GetUserInputToContinue();
            _consoleService.Clear();
        }

        AnsiConsole.MarkupLine($"You got {Score} out of {cards.Count}");
        _logger.LogInformation("Study session completed. Score: {Score}/{Total}", Score, cards.Count);
        _userInteractionService.GetUserInputToContinue();
        _consoleService.Clear();
    }

    public async Task<Result> RunStudySessionAsync(List<BaseCardDTO> studySessionCards, int stackId)
    {
        _logger.LogInformation("Running study session for stack {StackId}.", stackId);

        if (studySessionCards is [])
        {
            _logger.LogWarning("No cards found for study session.");
            return Result.Failure(CardsErrors.CardsNotFound);
        }

        StartStudySessionAsync(studySessionCards);
        var endResult = await EndStudySessionAsync(stackId);
        if (endResult.IsFailure)
        {
            _logger.LogWarning("Failed to end study session for stack {StackId}: {Error}", stackId, endResult.Error.Description);
            return Result.Failure(endResult.Error);
        }

        _logger.LogInformation("Study session for stack {StackId} completed successfully.", stackId);
        return Result.Success();
    }

    private async Task<Result> EndStudySessionAsync(int stackId)
    {
        _logger.LogInformation("Ending study session for stack {StackId}.", stackId);
        return await AddStudySessionAsync(stackId);
    }

    private async Task<Result> AddStudySessionAsync(int stackId)
    {
        DateTime date = DateTime.Now;
        _logger.LogInformation("Adding study session for stack {StackId} on {Date} with score {Score}.", stackId, date, Score);
        var addResult = await _studySessionsRepository.AddStudySessionAsync(stackId, date, Score);
        if (addResult.IsFailure)
        {
            _logger.LogWarning("Failed to add study session for stack {StackId}: {Error}", stackId, addResult.Error.Description);
            return Result.Failure(addResult.Error);
        }

        _logger.LogInformation("Study session for stack {StackId} added successfully.", stackId);
        return Result.Success();
    }

    public async Task<Result<List<StudySessionDTO>>> GetAllStudySessionsAsync()
    {
        _logger.LogInformation("Retrieving all study sessions.");
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure)
        {
            _logger.LogWarning("Failed to check for study sessions: {Error}", hasSessionResult.Error.Description);
            return Result.Failure<List<StudySessionDTO>>(hasSessionResult.Error);
        }
        if (!hasSessionResult.Value)
        {
            _logger.LogInformation("No study sessions found.");
            return Result.Success(new List<StudySessionDTO>());
        }

        var studySessionsResult = await _studySessionsRepository.GetAllStudySessionsAsync();
        if (studySessionsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve study sessions: {Error}", studySessionsResult.Error.Description);
            return Result.Failure<List<StudySessionDTO>>(studySessionsResult.Error);
        }

        List<StudySessionDTO> studySessionDtos = new();
        foreach (var studySession in studySessionsResult.Value)
        {
            studySessionDtos.Add(Mapper.ToStudySessionDTO(studySession));
        }

        _logger.LogInformation("Retrieved {Count} study sessions.", studySessionDtos.Count);
        return Result.Success(studySessionDtos);
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionsReportAsync()
    {
        _logger.LogInformation("Retrieving monthly study sessions report.");
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure)
        {
            _logger.LogWarning("Failed to check for study sessions: {Error}", hasSessionResult.Error.Description);
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(hasSessionResult.Error);
        }
        if (!hasSessionResult.Value)
        {
            _logger.LogInformation("No study sessions found for monthly report.");
            return Result.Success(Enumerable.Empty<MonthlyStudySessionsNumberData>());
        }

        string year = _userInteractionService.GetYear();
        _logger.LogInformation("User requested monthly study session report for year {Year}.", year);
        var reportResult = await _studySessionsRepository.GetMonthlyStudySessionReportAsync(year);
        if (reportResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve monthly study session report for year {Year}: {Error}", year, reportResult.Error.Description);
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(reportResult.Error);
        }

        _logger.LogInformation("Monthly study session report for year {Year} retrieved successfully.", year);
        return Result.Success(reportResult.Value);
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionsAverageScoreReportAsync()
    {
        _logger.LogInformation("Retrieving monthly average score report.");
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure)
        {
            _logger.LogWarning("Failed to check for study sessions: {Error}", hasSessionResult.Error.Description);
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(hasSessionResult.Error);
        }
        if (!hasSessionResult.Value)
        {
            _logger.LogInformation("No study sessions found for monthly average score report.");
            return Result.Success(Enumerable.Empty<MonthlyStudySessionsAverageScoreData>());
        }

        string year = _userInteractionService.GetYear();
        _logger.LogInformation("User requested monthly average score report for year {Year}.", year);
        var reportResult = await _studySessionsRepository.GetMonthlyStudySessionAverageScoreReportAsync(year);
        if (reportResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve monthly average score report for year {Year}: {Error}", year, reportResult.Error.Description);
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(reportResult.Error);
        }

        _logger.LogInformation("Monthly average score report for year {Year} retrieved successfully.", year);
        return Result.Success(reportResult.Value);
    }

    private bool IsCorrectMultipleChoiceCardAnswer(List<string> userAnswers, List<string> correctAnswers)
    {
        if (userAnswers.Count != correctAnswers.Count)
            return false;

        var userAnswersSet = new HashSet<string>(userAnswers);
        var correctAnswersSet = new HashSet<string>(correctAnswers);

        return userAnswersSet.SetEquals(correctAnswersSet);
    }
}
