using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Flashcards.UnitTests.Services;
public class StudySessionsServiceTests
{
    private readonly StudySessionsService _studySessionsService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStudySessionsRepository _studySessionsRepository;
    private readonly IConsoleService _consoleService;
    private readonly ILogger<StudySessionsService> _logger;

    public StudySessionsServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _studySessionsRepository = Substitute.For<IStudySessionsRepository>();
        _consoleService = Substitute.For<IConsoleService>();
        _logger = Substitute.For<ILogger<StudySessionsService>>();
        _studySessionsService = new StudySessionsService(
            _studySessionsRepository, _userInteractionService, _consoleService, _logger
        );
    }

    [Fact]
    public void StartStudySessionAsync_ShouldCalculateCorrectScore_WhenMixedCardTypes()
    {
        // Arrange
        var flashcard = new FlashcardDto { Id = 1, Front = "Hello", Back = "Hola", CardType = CardType.Flashcard };
        var multipleChoiceCard = new MultipleChoiceCardDto
        {
            Id = 2,
            Question = "Which is a fruit?",
            Choices = new List<string> { "Apple", "Car", "Book" },
            Answer = new List<string> { "Apple" },
            CardType = CardType.MultipleChoice
        };
        var clozeCard = new ClozeCardDto { Id = 3, ClozeText = "The capital of France is {{c1::Paris}}.", CardType = CardType.Cloze };
        var cards = new List<BaseCardDto> { flashcard, multipleChoiceCard, clozeCard };

        _userInteractionService.GetAnswer().Returns("Hola", "Paris");
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>())
            .Returns(new List<string> { "Apple" });

        // Act
        _studySessionsService.StartStudySessionAsync(cards);

        // Assert
        Assert.Equal(3, _studySessionsService.Score);
        _userInteractionService.Received(2).GetAnswer();
        _userInteractionService.Received(1).GetMultipleChoiceAnswers(multipleChoiceCard.Choices);
    }

    [Fact]
    public async Task RunStudySessionAsync_ShouldReturnFailure_WhenCardsEmpty()
    {
        // Arrange
        List<BaseCardDto> cards = new();
        int stackId = 1;

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(cards, stackId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CardsErrors.CardsNotFound, result.Error);
    }

    [Fact]
    public async Task RunStudySessionAsync_ShouldReturnFailure_WhenEndStudySessionAsyncFails()
    {
        // Arrange
        List<BaseCardDto> cards = new()
        {
            new FlashcardDto() { Front = "Front1", Back = "Back1" }
        };
        int stackId = 1;

        _userInteractionService.GetAnswer().Returns("Back1");
        _studySessionsRepository.AddStudySessionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(Result.Failure(StudySessionsErrors.AddFailed));

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(cards, stackId);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RunStudySessionAsync_ShouldReturnSuccess_WhenValidCardsProvided()
    {
        // Arrange
        List<BaseCardDto> cards = new()
        {
            new FlashcardDto() { Front = "Front1", Back = "Back1" }
        };
        int stackId = 1;

        _userInteractionService.GetAnswer().Returns("Back1");
        _studySessionsRepository.AddStudySessionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(Result.Success());

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(cards, stackId);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAllStudySessionsAsync_ShouldReturnFailure_WhenRepositoryGetAllFails()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(true));
        _studySessionsRepository.GetAllStudySessionsAsync().Returns(Result.Failure<IEnumerable<StudySession>>(StudySessionsErrors.GetFailed));

        // Act
        var result = await _studySessionsService.GetAllStudySessionsAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(StudySessionsErrors.GetFailed, result.Error);
    }

    [Fact]
    public async Task GetAllStudySessionsAsync_ShouldReturnEmptyList_WhenNoSessionsExist()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(false));
        _studySessionsRepository.GetAllStudySessionsAsync().Returns(Result.Success<IEnumerable<StudySession>>([]));

        // Act
        var result = await _studySessionsService.GetAllStudySessionsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllStudySessionsAsync_ShouldReturnMappedDTOs_WhenSessionsExist()
    {
        // Arrange
        var studySessions = new List<StudySession>
        {
            new StudySession { Id = 1, StackId = 1, Date = DateTime.Now, Score = 5 },
            new StudySession { Id = 2, StackId = 2, Date = DateTime.Now.AddDays(-1), Score = 3 }
        };
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(true));
        _studySessionsRepository.GetAllStudySessionsAsync().Returns(Result.Success<IEnumerable<StudySession>>(studySessions));

        // Act
        var result = await _studySessionsService.GetAllStudySessionsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(1, result.Value[0].Id);
        Assert.Equal(2, result.Value[1].Id);
    }

    [Fact]
    public void StartStudySessionAsync_ShouldResetScore_WhenCalledMultipleTimes()
    {
        // Arrange
        var flashcard1 = new FlashcardDto { Id = 1, Front = "Hello", Back = "Hola" };
        var flashcard2 = new FlashcardDto { Id = 2, Front = "Goodbye", Back = "Adiós" };
        var cards1 = new List<BaseCardDto> { flashcard1 };
        var cards2 = new List<BaseCardDto> { flashcard2 };

        _userInteractionService.GetAnswer().Returns("Hola", "Adiós");

        // Act
        _studySessionsService.StartStudySessionAsync(cards1);
        var scoreAfterFirst = _studySessionsService.Score;

        _studySessionsService.StartStudySessionAsync(cards2);
        var scoreAfterSecond = _studySessionsService.Score;

        // Assert
        Assert.Equal(1, scoreAfterFirst);
        Assert.Equal(1, scoreAfterSecond);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsReportAsync_ShouldReturnFailure_WhenRepositoryGetMonthlyReportFails()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(true));
        _studySessionsRepository.GetMonthlyStudySessionReportAsync(Arg.Any<string>())
            .Returns(Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(StudySessionsErrors.ReportFailed));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsReportAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(StudySessionsErrors.ReportFailed, result.Error);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsReportAsync_ShouldReturnSuccess_WhenValidDataExists()
    {
        // Arrange
        var monthlyData = new List<MonthlyStudySessionsNumberData>
        {
            new() {
                StackName = "Test",
                JanuaryNumber = 5,
                FebruaryNumber = 3,
                MarchNumber = 4,
                AprilNumber = 2,
                MayNumber = 6,
                JuneNumber = 1,
                JulyNumber = 0,
                AugustNumber = 7,
                SeptemberNumber = 8,
                OctoberNumber = 9,
                NovemberNumber = 10,
                DecemberNumber = 11
            },
            new() {
                StackName = "Test2",
                JanuaryNumber = 1,
                FebruaryNumber = 2,
                MarchNumber = 3,
                AprilNumber = 4,
                MayNumber = 5,
                JuneNumber = 6,
                JulyNumber = 7,
                AugustNumber = 8,
                SeptemberNumber = 9,
                OctoberNumber = 10,
                NovemberNumber = 11,
                DecemberNumber = 12
            }

        };
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(true));
        _studySessionsRepository.GetMonthlyStudySessionReportAsync(Arg.Any<string>())
            .Returns(Result.Success<IEnumerable<MonthlyStudySessionsNumberData>>(monthlyData));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsReportAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsReportAsync_ShouldReturnEmptyCollection_WhenNoSessionsExist()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(false));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsReportAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsReportAsync_ShouldReturnFailure_WhenHasStudySessionsFails()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Failure<bool>(StudySessionsErrors.HasStudySessionFailed));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsReportAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(StudySessionsErrors.HasStudySessionFailed, result.Error);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsAverageScoreReportAsync_ShouldReturnEmptyReport_WhenNoSessionsExist()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(false));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsAverageScoreReportAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsAverageScoreReportAsync_ShouldReturnFailure_WhenHasStudySessionsFails()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Failure<bool>(StudySessionsErrors.HasStudySessionFailed));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsAverageScoreReportAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(StudySessionsErrors.HasStudySessionFailed, result.Error);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsAverageScoreReportAsync_ShouldReturnFailure_WhenRepositoryGetMonthlyAverageScoreReportFails()
    {
        // Arrange
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(true));
        _studySessionsRepository.GetMonthlyStudySessionAverageScoreReportAsync(Arg.Any<string>())
            .Returns(Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(StudySessionsErrors.ReportFailed));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsAverageScoreReportAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(StudySessionsErrors.ReportFailed, result.Error);
    }

    [Fact]
    public async Task GetMonthlyStudySessionsAverageScoreReportAsync_ShouldReturnSuccess_WhenValidDataExists()
    {
        // Arrange
        var monthlyData = new List<MonthlyStudySessionsAverageScoreData>
        {
            new() {
                StackName = "Test",
                JanuaryAverageScore = 5,
                FebruaryAverageScore = 3,
                MarchAverageScore = 4,
                AprilAverageScore = 2,
                MayAverageScore = 6,
                JuneAverageScore = 1,
                JulyAverageScore = 0,
                AugustAverageScore = 7,
                SeptemberAverageScore = 8,
                OctoberAverageScore = 9,
                NovemberAverageScore = 10,
                DecemberAverageScore = 11
            },
            new() {
                StackName = "Test2",
                JanuaryAverageScore = 1,
                FebruaryAverageScore = 2,
                MarchAverageScore = 3,
                AprilAverageScore = 4,
                MayAverageScore = 5,
                JuneAverageScore = 6,
                JulyAverageScore = 7,
                AugustAverageScore = 8,
                SeptemberAverageScore = 9,
                OctoberAverageScore = 10,
                NovemberAverageScore = 11,
                DecemberAverageScore = 12
            }
        };
        _studySessionsRepository.HasStudySessionAsync().Returns(Result.Success(true));
        _studySessionsRepository.GetMonthlyStudySessionAverageScoreReportAsync(Arg.Any<string>())
            .Returns(Result.Success<IEnumerable<MonthlyStudySessionsAverageScoreData>>(monthlyData));

        // Act
        var result = await _studySessionsService.GetMonthlyStudySessionsAverageScoreReportAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }
}