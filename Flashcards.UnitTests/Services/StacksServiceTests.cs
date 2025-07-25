using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using NSubstitute;
using System.Reflection;

namespace Flashcards.UnitTests.Services;
public class StacksServiceTests
{
    private readonly StacksService _stacksService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository _stacksRepository;
    private readonly IFlashcardsRepository _flashcardsRepository;

    public StacksServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _stacksRepository = Substitute.For<IStacksRepository>();
        _flashcardsRepository = Substitute.For<IFlashcardsRepository>();
        _stacksService = new StacksService(_stacksRepository, _userInteractionService, _flashcardsRepository);
    }

    private Stack CreateTestStack(int id = 1, string name = "Test Stack") =>
        new() { Id = id, Name = name };

    private Flashcard CreateTestFlashcard(int id = 1, int stackId = 1, string front = "Front", string back = "Back") =>
        new() { Id = id, StackId = stackId, Front = front, Back = back };

    private FlashcardDTO CreateTestFlashcardDTO(int id = 1, string front = "Front", string back = "Back") =>
        new() { Id = id, Front = front, Back = back };

    [Fact]
    public async Task AddStackAsync_ShouldReturnFailure_WhenStackWithNameExistsFails()
    {
        // Arrange
        string stackName = "Test Stack";
        _userInteractionService.GetStackName().Returns(stackName);
        _stacksRepository.StackExistsWithNameAsync(stackName)
            .Returns(Result.Failure<bool>(StacksErrors.StackWithNameFailed));

        // Act
        var result = await _stacksService.AddStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.StackWithNameFailed, result.Error);
    }

    [Fact]
    public async Task AddStackAsync_ShouldReturnFailure_WhenAddStackFails()
    {
        // Arrange
        string stackName = "Test Stack";
        _userInteractionService.GetStackName().Returns(stackName);
        _stacksRepository.StackExistsWithNameAsync(stackName).Returns(Result.Success(false));
        _stacksRepository.AddStackAsync(stackName).Returns(Result.Failure(StacksErrors.AddFailed));

        // Act
        var result = await _stacksService.AddStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddStackAsync_ShouldReturnSuccess_WhenAddStackSucceeds()
    {
        // Arrange
        string stackName = "Test Stack";
        _userInteractionService.GetStackName().Returns(stackName);
        _stacksRepository.StackExistsWithNameAsync(stackName).Returns(Result.Success(false));
        _stacksRepository.AddStackAsync(stackName).Returns(Result.Success());

        // Act
        var result = await _stacksService.AddStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddFlashcardToStackAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Arrange
        string front = "Front";
        string back = "Back";
        _userInteractionService.GetFlashcardFront().Returns(front);
        _userInteractionService.GetFlashcardBack().Returns(back);

        // Act
        var result = await _stacksService.AddFlashcardToStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task AddFlashcardToStackAsync_ShouldReturnFailure_WhenAddFlashcardFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        string front = "Front";
        string back = "Back";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetFlashcardFront().Returns(front);
        _userInteractionService.GetFlashcardBack().Returns(back);
        _flashcardsRepository.AddFlashcardAsync(stackId, front, back)
            .Returns(Result.Failure(FlashcardsErrors.AddFailed));

        // Act
        var result = await _stacksService.AddFlashcardToStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddFlashcardToStackAsync_ShouldReturnSuccess_WhenAddFlashcardSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        string front = "Front";
        string back = "Back";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetFlashcardFront().Returns(front);
        _userInteractionService.GetFlashcardBack().Returns(back);
        _flashcardsRepository.AddFlashcardAsync(stackId, front, back)
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.AddFlashcardToStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteStackAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Act
        var result = await _stacksService.DeleteStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteStackAsync_ShouldReturnFailure_WhenDeleteStackFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _stacksRepository.DeleteStackAsync(stackId).Returns(Result.Failure(StacksErrors.DeleteStackFailed));

        // Act
        var result = await _stacksService.DeleteStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.DeleteStackFailed, result.Error);
        await _stacksRepository.Received(1).DeleteStackAsync(stackId);
    }

    [Fact]
    public async Task DeleteStackAsync_ShouldReturnSuccess_WhenDeleteStackSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.DeleteStackAsync(stackId).Returns(Result.Success());

        // Act
        var result = await _stacksService.DeleteStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        await _stacksRepository.Received(1).DeleteStackAsync(stackId);
    }

    [Fact]
    public async Task DeleteFlashcardFromStackAsync_ShouldReturnFailure_WhenGetAllFlashcardsByStackIdFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId)
            .Returns(Result.Failure<IEnumerable<Flashcard>>(StacksErrors.GetFlashcardsByStackIdFailed));

        // Act
        var result = await _stacksService.DeleteFlashcardFromStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetFlashcardsByStackIdFailed, result.Error);
    }

    [Fact]
    public async Task DeleteFlashcardFromStackAsync_ShouldReturnFailure_WhenFlashcardsEmpty()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<Flashcard>>([]));

        // Act
        var result = await _stacksService.DeleteFlashcardFromStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.FlashcardsNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteFlashcardFromStackAsync_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int flashcardId = 1;
        string front = "Front";
        string back = "Back";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var flashcards = new List<Flashcard> { CreateTestFlashcard(flashcardId, stackId, front, back) };
        var flashcardsDTO = new List<FlashcardDTO> { CreateTestFlashcardDTO(flashcardId, front, back) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>()).Returns(flashcardsDTO[0]);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        _stacksRepository.DeleteFlashcardFromStackAsync(flashcardId, stackId).Returns(Result.Failure(StacksErrors.DeleteFlashcardFailed));

        // Act
        var result = await _stacksService.DeleteFlashcardFromStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.DeleteFlashcardFailed, result.Error);
        await _stacksRepository.Received(1).DeleteFlashcardFromStackAsync(flashcardId, stackId);
    }

    [Fact]
    public async Task DeleteFlashcardFromStackAsync_ShouldReturnSuccess_WhenDeleteSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int flashcardId = 1;
        string front = "Front";
        string back = "Back";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var flashcards = new List<Flashcard> { CreateTestFlashcard(flashcardId, stackId, front, back) };
        var flashcardsDTO = new List<FlashcardDTO> { CreateTestFlashcardDTO(flashcardId, front, back) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>()).Returns(flashcardsDTO[0]);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        _stacksRepository.DeleteFlashcardFromStackAsync(flashcardId, stackId).Returns(Result.Success());

        // Act
        var result = await _stacksService.DeleteFlashcardFromStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        await _stacksRepository.Received(1).DeleteFlashcardFromStackAsync(flashcardId, stackId);
    }

    [Fact]
    public async Task UpdateFlashcardInStackAsync_ShouldReturnFailure_WhenGetFlashcardsByStackIdFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId)
            .Returns(Result.Failure<IEnumerable<Flashcard>>(StacksErrors.GetFlashcardsByStackIdFailed));

        // Act
        var result = await _stacksService.UpdateFlashcardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetFlashcardsByStackIdFailed, result.Error);
    }

    [Fact]
    public async Task UpdateFlashcardInStackAsync_ShouldReturnFailure_WhenFlashcardsEmpty()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<Flashcard>>([]));

        // Act
        var result = await _stacksService.UpdateFlashcardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FlashcardsErrors.FlashcardsNotFound, result.Error);
    }

    [Fact]
    public async Task UpdateFlashcardInStackAsync_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int flashcardId = 1;
        string originalFront = "Original Front";
        string originalBack = "Original Back";
        string newFront = "New Front";
        string newBack = "New Back";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var flashcards = new List<Flashcard> { CreateTestFlashcard(flashcardId, stackId, originalFront, originalBack) };
        var flashcardsDTO = new List<FlashcardDTO> { CreateTestFlashcardDTO(flashcardId, originalFront, originalBack) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>()).Returns(flashcardsDTO[0]);
        _userInteractionService.GetFlashcardFront().Returns(newFront);
        _userInteractionService.GetFlashcardBack().Returns(newBack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        _stacksRepository.UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack)
            .Returns(Result.Failure(StacksErrors.UpdateFailed));

        // Act
        var result = await _stacksService.UpdateFlashcardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.UpdateFailed, result.Error);
        await _stacksRepository.Received(1).UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack);
    }

    [Fact]
    public async Task UpdateFlashcardInStackAsync_ShouldReturnSuccess_WhenUpdateSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int flashcardId = 1;
        string originalFront = "Original Front";
        string originalBack = "Original Back";
        string newFront = "New Front";
        string newBack = "New Back";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var flashcards = new List<Flashcard> { CreateTestFlashcard(flashcardId, stackId, originalFront, originalBack) };
        var flashcardsDTO = new List<FlashcardDTO> { CreateTestFlashcardDTO(flashcardId, originalFront, originalBack) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>()).Returns(flashcardsDTO[0]);
        _userInteractionService.GetFlashcardFront().Returns(newFront);
        _userInteractionService.GetFlashcardBack().Returns(newBack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));
        _stacksRepository.UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack)
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.UpdateFlashcardInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        await _stacksRepository.Received(1).UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack);
    }

    [Fact]
    public async Task GetFlashcardsByStackIdAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Act
        var result = await _stacksService.GetFlashcardsByStackIdAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task GetFlashcardsByStackIdAsync_ShouldReturnFailure_WhenRepositoryGetFlashcardsByStackIdFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId)
            .Returns(Result.Failure<IEnumerable<Flashcard>>(StacksErrors.GetFlashcardsByStackIdFailed));

        // Act
        var result = await _stacksService.GetFlashcardsByStackIdAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetFlashcardsByStackIdFailed, result.Error);
    }

    [Fact]
    public async Task GetFlashcardsByStackIdAsync_ShouldReturnMappedFlashcards_WhenRepositorySucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var flashcards = new List<Flashcard>
        {
            CreateTestFlashcard(1, stackId, "Front1", "Back1"),
            CreateTestFlashcard(2, stackId, "Front2", "Back2")
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<Flashcard>>(flashcards));

        // Act
        var result = await _stacksService.GetFlashcardsByStackIdAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(1, result.Value[0].Id);
        Assert.Equal("Front1", result.Value[0].Front);
        Assert.Equal("Back1", result.Value[0].Back);
        Assert.Equal(2, result.Value[1].Id);
        Assert.Equal("Front2", result.Value[1].Front);
        Assert.Equal("Back2", result.Value[1].Back);
    }

    [Fact]
    public async Task GetFlashcardsCountInStackAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Act
        var result = await _stacksService.GetFlashcardsCountInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task GetFlashcardsCountInStackAsync_ShouldReturnFailure_WhenRepositoryGetFlashcardsCountFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsCountInStackAsync(stackId)
            .Returns(Result.Failure<int>(StacksErrors.GetFlashcardsCountFailed));

        // Act
        var result = await _stacksService.GetFlashcardsCountInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetFlashcardsCountFailed, result.Error);
    }

    [Fact]
    public async Task GetFlashcardsCountInStackAsync_ShouldReturnCount_WhenRepositorySucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int expectedCount = 5;
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetFlashcardsCountInStackAsync(stackId)
            .Returns(Result.Success(expectedCount));

        // Act
        var result = await _stacksService.GetFlashcardsCountInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCount, result.Value);
    }

    [Fact]
    public async Task GetAllStacksAsync_ShouldReturnFailure_WhenRepositoryGetAllStacksFails()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed));

        // Act
        var result = await _stacksService.GetAllStacksAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetStacksFailed, result.Error);
    }

    [Fact]
    public async Task GetAllStacksAsync_ShouldReturnMappedStackDTOs_WhenRepositorySucceeds()
    {
        // Arrange
        var stacks = new List<Stack>
        {
            CreateTestStack(1, "Stack1"),
            CreateTestStack(2, "Stack2")
        };
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Success<IEnumerable<Stack>>(stacks));

        // Act
        var result = await _stacksService.GetAllStacksAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Stack1", result.Value[0].Name);
        Assert.Equal("Stack2", result.Value[1].Name);
    }

    [Fact]
    public async Task GetStackAsync_ShouldReturnFailure_WhenGetAllStacksFails()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed));

        // Act
        var result = await _stacksService.GetStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetStacksFailed, result.Error);
    }

    [Fact]
    public async Task GetStackAsync_ShouldReturnFailure_WhenNoStacksExist()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Success<IEnumerable<Stack>>([]));

        // Act
        var result = await _stacksService.GetStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.StacksNotFound, result.Error);
    }

    [Fact]
    public async Task GetStackAsync_ShouldReturnFailure_WhenRepositoryGetStackFails()
    {
        // Arrange
        string stackName = "Test Stack";
        var stacks = new List<Stack> { CreateTestStack(1, stackName) };
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Success<IEnumerable<Stack>>(stacks));
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns(stackName);
        _stacksRepository.GetStackAsync(stackName)
            .Returns(Result.Failure<Stack>(StacksErrors.GetStackFailed));

        // Act
        var result = await _stacksService.GetStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetStackFailed, result.Error);
    }

    [Fact]
    public async Task GetStackAsync_ShouldSetCurrentStackAndReturnSuccess_WhenRepositorySucceeds()
    {
        // Arrange
        string stackName = "Test Stack";
        var stack = CreateTestStack(1, stackName);
        var stacks = new List<Stack> { stack };
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Success<IEnumerable<Stack>>(stacks));
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns(stackName);
        _stacksRepository.GetStackAsync(stackName)
            .Returns(Result.Success(stack));

        // Act
        var result = await _stacksService.GetStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var currentStack = typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(_stacksService) as Stack;
        Assert.NotNull(currentStack);
        Assert.Equal(stack.Id, currentStack.Id);
        Assert.Equal(stack.Name, currentStack.Name);
    }

    [Fact]
    public void GetCurrentStack_ShouldReturnCurrentStack_WhenCurrentStackIsSet()
    {
        // Arrange
        var testStack = CreateTestStack(1, "Test Stack");
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, testStack);

        // Act
        var result = _stacksService.GetCurrentStack();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testStack.Id, result.Id);
        Assert.Equal(testStack.Name, result.Name);
    }
}
