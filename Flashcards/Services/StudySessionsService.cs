using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace Flashcards.Services;

public class StudySessionsService : IStudySessionsService
{
    private readonly IStudySessionsRepository _studySessionsRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IConsoleService _consoleService;
    private readonly ILogger<StudySessionsService> _logger;
    public int Score { get; private set; }

    public StudySessionsService(
        IStudySessionsRepository studySessionsRepository,
        ICardsRepository cardsRepository,
        IUserInteractionService userInteractionService,
        IConsoleService consoleService,
        ILogger<StudySessionsService> logger)
    {
        _studySessionsRepository = studySessionsRepository;
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
        _consoleService = consoleService;
        _logger = logger;
    }

    public async Task<Result> StartStudySessionAsync(List<BaseCardDto> cards)
    {
        _logger.LogInformation("Starting study session with {Count} cards.", cards.Count);
        var updatedCards = new List<CardProgressUpdateDto>();
        Score = 0;
        foreach (BaseCardDto card in cards)
        {
            IStudyCardStrategy strategy = card.CardType switch
            {
                CardType.Flashcard => new FlashcardStudyStrategy(_userInteractionService, _logger),
                CardType.MultipleChoice => new MultipleChoiceCardStudyStrategy(_userInteractionService, _logger),
                CardType.Cloze => new ClozeCardStudyStrategy(_userInteractionService, _logger),
                _ => throw new NotSupportedException($"Card type {card.CardType} is not supported.")
            };

            bool isCorrect = strategy.Study(card);
            int newBox = GetNextBox(isCorrect, card.Box);
            DateTime newReviewDate = GetNextReviewDate(isCorrect, newBox);

            var updatedCard = new CardProgressUpdateDto
            {
                CardId = card.Id,
                Box = newBox,
                NextReviewDate = newReviewDate
            };
            updatedCards.Add(updatedCard);

            if (isCorrect) Score++;

            _userInteractionService.GetUserInputToContinue();
            _consoleService.Clear();
        }

        var result = await _cardsRepository.UpdateCardsProgressBulkAsync(updatedCards);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update card progress: {Error}", result.Error.Description);
            return Result.Failure(result.Error);
        }

        AnsiConsole.MarkupLine($"You got {Score} out of {cards.Count}");
        _logger.LogInformation("Study session completed. Score: {Score}/{Total}", Score, cards.Count);
        _userInteractionService.GetUserInputToContinue();
        _consoleService.Clear();

        return Result.Success();
    }

    public async Task<Result> RunStudySessionAsync(List<BaseCardDto> studySessionCards, int stackId)
    {
        _logger.LogInformation("Running study session for stack {StackId}.", stackId);

        if (studySessionCards is [])
        {
            _logger.LogWarning("No cards found for study session.");
            return Result.Failure(CardsErrors.CardsNotFound);
        }

        var studyResult = await StartStudySessionAsync(studySessionCards);

        if (studyResult.IsFailure)
        {
            _logger.LogWarning("Study session failed: {Error}", studyResult.Error.Description);
            return Result.Failure(studyResult.Error);
        }

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

    public async Task<Result<List<StudySessionDto>>> GetAllStudySessionsAsync()
    {
        _logger.LogInformation("Retrieving all study sessions.");
        var hasSessionResult = await _studySessionsRepository.HasStudySessionAsync();
        if (hasSessionResult.IsFailure)
        {
            _logger.LogWarning("Failed to check for study sessions: {Error}", hasSessionResult.Error.Description);
            return Result.Failure<List<StudySessionDto>>(hasSessionResult.Error);
        }
        if (!hasSessionResult.Value)
        {
            _logger.LogInformation("No study sessions found.");
            return Result.Success(new List<StudySessionDto>());
        }

        var studySessionsResult = await _studySessionsRepository.GetAllStudySessionsAsync();
        if (studySessionsResult.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve study sessions: {Error}", studySessionsResult.Error.Description);
            return Result.Failure<List<StudySessionDto>>(studySessionsResult.Error);
        }

        List<StudySessionDto> studySessionDtos = Mapper.ToStudySessionDTOList(studySessionsResult.Value);

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

    private static int GetNextBox(bool isCorrectAnswer, int currentBox)
    {
        return isCorrectAnswer ? Math.Min(currentBox + 1, 3) : 1;
    }

    private static DateTime GetNextReviewDate(bool isCorrectAnswer, int currentBox)
    {
        return isCorrectAnswer ? DateTime.Now.AddDays(currentBox switch
        {
            1 => 1,
            2 => 3,
            3 => 7,
            _ => 1
        }) : DateTime.Now;
    }
}