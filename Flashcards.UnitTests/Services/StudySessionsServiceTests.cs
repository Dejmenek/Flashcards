using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using NSubstitute;

namespace Flashcards.UnitTests.Services;
public class StudySessionsServiceTests
{
    private readonly StudySessionsService _studySessionsService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStudySessionsRepository _studySessionsRepository;
    private readonly IConsoleService _consoleService;

    public StudySessionsServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _studySessionsRepository = Substitute.For<IStudySessionsRepository>();
        _consoleService = Substitute.For<IConsoleService>();
        _studySessionsService = new StudySessionsService(_studySessionsRepository, _userInteractionService, _consoleService);
    }

    public static IEnumerable<object[]> StartStudySessionTestData =>
        new List<object[]>
        {
            new object[]
            {
                new List<FlashcardDTO>
                {
                    new() { Front = "Dog", Back = "Pies" },
                    new() { Front = "Cat", Back = "Kot" }
                },
                new List<string> { "Pies", "Kot" },
                2
            },
            new object[]
            {
                new List<FlashcardDTO>
                {
                    new() { Front = "Front3", Back = "Back3" }
                },
                new List<string> { "Back3" },
                1
            },
            new object[]
            {
                new List<FlashcardDTO>(),
                new List<string>(),
                0
            },
            new object[]
            {
                new List<FlashcardDTO>
                {
                    new() { Front = "Front4", Back = "Back4" },
                },
                new List<string> { "WrongAnswer" },
                0
            }
        };

    [Theory]
    [MemberData(nameof(StartStudySessionTestData))]
    public void StartStudySessionAsync_ShouldCalculateCorrectScore_WhenAnswersProvided(List<FlashcardDTO> flashcards, List<string> userAnswers, int expectedScore)
    {
        // Arrange
        int i = 0;
        _userInteractionService.GetAnswer().Returns(_ => userAnswers[i++]);

        // Act
        _studySessionsService.StartStudySessionAsync(flashcards);

        // Assert
        Assert.Equal(expectedScore, _studySessionsService.Score);
    }

    [Fact]
    public async Task RunStudySessionAsync_ShouldReturnFailure_WhenFlashcardsEmpty()
    {
        // Arrange
        List<FlashcardDTO> flashcards = new();
        int stackId = 1;

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(flashcards, stackId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(FlashcardsErrors.FlashcardsNotFound, result.Error);
    }

    [Fact]
    public async Task RunStudySessionAsync_ShouldReturnFailure_WhenEndStudySessionAsyncFails()
    {
        // Arrange
        List<FlashcardDTO> flashcards = new()
        {
            new() { Front = "Front1", Back = "Back1" }
        };
        int stackId = 1;

        _userInteractionService.GetAnswer().Returns("Back1");
        _studySessionsRepository.AddStudySessionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(Result.Failure(StudySessionsErrors.AddFailed));

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(flashcards, stackId);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RunStudySessionAsync_ShouldReturnSuccess_WhenValidFlashcardsProvided()
    {
        // Arrange
        List<FlashcardDTO> flashcards = new()
        {
            new() { Front = "Front1", Back = "Back1" }
        };
        int stackId = 1;

        _userInteractionService.GetAnswer().Returns("Back1");
        _studySessionsRepository.AddStudySessionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>())
            .Returns(Result.Success());

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(flashcards, stackId);

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
