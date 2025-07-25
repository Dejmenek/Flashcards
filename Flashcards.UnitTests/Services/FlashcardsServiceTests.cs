using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using NSubstitute;

namespace Flashcards.UnitTests.Services;
public class FlashcardsServiceTests
{
    private readonly FlashcardsService _flashcardsService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository _stacksRepository;
    private readonly IFlashcardsRepository _flashcardsRepository;

    public FlashcardsServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _stacksRepository = Substitute.For<IStacksRepository>();
        _flashcardsRepository = Substitute.For<IFlashcardsRepository>();
        _flashcardsService = new FlashcardsService(_flashcardsRepository, _userInteractionService, _stacksRepository);
    }

    [Fact]
    public async Task AddFlashcardAsync_ShouldReturnFailure_WhenGetAllStacksFails()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed));

        // Act
        var result = await _flashcardsService.AddFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetStacksFailed, result.Error);
    }

    [Fact]
    public async Task AddFlashcardAsync_ShouldReturnFailure_WhenNoStacksExist()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success(Enumerable.Empty<Stack>()));

        // Act
        var result = await _flashcardsService.AddFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.StacksNotFound, result.Error);
    }

    [Fact]
    public async Task AddFlashcardAsync_ShouldReturnFailure_WhenRepositoryAddFlashcardFails()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns("Test Stack");
        _userInteractionService.GetFlashcardFront().Returns("Front Text");
        _userInteractionService.GetFlashcardBack().Returns("Back Text");
        _flashcardsRepository.AddFlashcardAsync(1, "Front Text", "Back Text")
            .Returns(Result.Failure(FlashcardsErrors.AddFailed));

        // Act
        var result = await _flashcardsService.AddFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddFlashcardAsync_ShouldReturnSuccess_WhenValidInputProvided()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns("Test Stack");
        _userInteractionService.GetFlashcardFront().Returns("Front Text");
        _userInteractionService.GetFlashcardBack().Returns("Back Text");
        _flashcardsRepository.AddFlashcardAsync(1, "Front Text", "Back Text").Returns(Result.Success());

        // Act
        var result = await _flashcardsService.AddFlashcardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteFlashcardAsync_ShouldReturnFailure_WhenGetAllFlashcardsFails()
    {
        // Arrange
        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Failure<IEnumerable<Flashcard>>(FlashcardsErrors.GetAllFailed));

        // Act
        var result = await _flashcardsService.DeleteFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.GetAllFailed, result.Error);
    }

    [Fact]
    public async Task DeleteFlashcardAsync_ShouldReturnFailure_WhenNoFlashcardsExist()
    {
        // Arrange
        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Success(Enumerable.Empty<Flashcard>()));

        // Act
        var result = await _flashcardsService.DeleteFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.FlashcardsNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteFlashcardAsync_ShouldReturnFailure_WhenRepositoryDeleteFails()
    {
        // Arrange
        var flashcards = new List<Flashcard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedFlashcard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };
        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>())
            .Returns(userSelectedFlashcard);
        _flashcardsRepository.DeleteFlashcardAsync(1)
            .Returns(Result.Failure(FlashcardsErrors.DeleteFailed));

        // Act
        var result = await _flashcardsService.DeleteFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.DeleteFailed, result.Error);
    }

    [Fact]
    public async Task DeleteFlashcardAsync_ShouldReturnSuccess_WhenValidFlashcardSelected()
    {
        // Arrange
        var flashcards = new List<Flashcard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedFlashcard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };
        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>())
            .Returns(userSelectedFlashcard);
        _flashcardsRepository.DeleteFlashcardAsync(1).Returns(Result.Success());

        // Act
        var result = await _flashcardsService.DeleteFlashcardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAllFlashcardsAsync_ShouldReturnFailure_WhenRepositoryGetAllFlashcardsFails()
    {
        // Arrange
        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Failure<IEnumerable<Flashcard>>(FlashcardsErrors.GetAllFailed));

        // Act
        var result = await _flashcardsService.GetAllFlashcardsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.GetAllFailed, result.Error);
    }

    [Fact]
    public async Task GetAllFlashcardsAsync_ShouldReturnMappedFlashcards_WhenFlashcardsExist()
    {
        // Arrange
        var flashcards = new List<Flashcard>
        {
            new Flashcard { Id = 1, StackId = 1, Front = "Front Text 1", Back = "Back Text 1" },
            new Flashcard { Id = 2, StackId = 2, Front = "Front Text 2", Back = "Back Text 2" }
        };

        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        var expectedFlashcards = flashcards.Select(f => new FlashcardDTO { Id = f.Id, Front = f.Front, Back = f.Back }).ToList();

        // Act
        var result = await _flashcardsService.GetAllFlashcardsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFlashcards.Count, result.Value.Count);
        for (int i = 0; i < expectedFlashcards.Count; i++)
        {
            Assert.Equal(expectedFlashcards[i].Id, result.Value[i].Id);
            Assert.Equal(expectedFlashcards[i].Front, result.Value[i].Front);
            Assert.Equal(expectedFlashcards[i].Back, result.Value[i].Back);
        }
    }

    [Fact]
    public async Task UpdateFlashcardAsync_ShouldReturnFailure_WhenGetAllFlashcardsFails()
    {
        // Arrange
        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Failure<IEnumerable<Flashcard>>(FlashcardsErrors.GetAllFailed));

        // Act
        var result = await _flashcardsService.UpdateFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.GetAllFailed, result.Error);
    }

    [Fact]
    public async Task UpdateFlashcardAsync_ShouldReturnFailure_WhenRepositoryUpdateFails()
    {
        // Arrange
        var flashcards = new List<Flashcard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedFlashcard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };

        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>())
            .Returns(userSelectedFlashcard);
        _userInteractionService.GetFlashcardFront().Returns("Updated Front");
        _userInteractionService.GetFlashcardBack().Returns("Updated Back");

        _flashcardsRepository.UpdateFlashcardAsync(1, "Updated Front", "Updated Back")
            .Returns(Result.Failure(FlashcardsErrors.UpdateFailed));

        // Act
        var result = await _flashcardsService.UpdateFlashcardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.UpdateFailed, result.Error);
    }

    [Fact]
    public async Task UpdateFlashcardAsync_ShouldReturnSuccess_WhenValidUpdateDataProvided()
    {
        // Arrange
        var flashcards = new List<Flashcard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedFlashcard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };

        _flashcardsRepository.GetAllFlashcardsAsync()
            .Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>())
            .Returns(userSelectedFlashcard);
        _userInteractionService.GetFlashcardFront().Returns("Updated Front");
        _userInteractionService.GetFlashcardBack().Returns("Updated Back");

        _flashcardsRepository.UpdateFlashcardAsync(1, "Updated Front", "Updated Back")
            .Returns(Result.Success());

        // Act
        var result = await _flashcardsService.UpdateFlashcardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }
}
