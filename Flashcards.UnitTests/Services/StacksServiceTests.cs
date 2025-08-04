using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Reflection;

namespace Flashcards.UnitTests.Services;
public class StacksServiceTests
{
    private readonly StacksService _stacksService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly ILogger<StacksService> _logger;

    public StacksServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _stacksRepository = Substitute.For<IStacksRepository>();
        _cardsRepository = Substitute.For<ICardsRepository>();
        _logger = Substitute.For<ILogger<StacksService>>();
        _stacksService = new StacksService(
            _stacksRepository, _userInteractionService, _cardsRepository, _logger
        );
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
    public async Task AddCardToStackAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Act
        var result = await _stacksService.AddCardToStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task AddCardToStackAsync_ShouldReturnFailure_WhenAddFlashcardFails()
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

        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _userInteractionService.GetFlashcardFront().Returns(front);
        _userInteractionService.GetFlashcardBack().Returns(back);

        _cardsRepository.AddFlashcardAsync(stackId, front, back)
            .Returns(Result.Failure(CardsErrors.AddFailed));

        // Act
        var result = await _stacksService.AddCardToStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddCardToStackAsync_ShouldReturnSuccess_WhenAddFlashcardSucceeds()
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

        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _userInteractionService.GetFlashcardFront().Returns(front);
        _userInteractionService.GetFlashcardBack().Returns(back);

        _cardsRepository.AddFlashcardAsync(stackId, front, back)
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.AddCardToStackAsync();

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
    public async Task DeleteCardFromStackAsync_ShouldReturnFailure_WhenGetAllCardsByStackIdFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Failure<IEnumerable<BaseCard>>(StacksErrors.GetCardsByStackIdFailed));

        // Act
        var result = await _stacksService.DeleteCardFromStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetCardsByStackIdFailed, result.Error);
    }

    [Fact]
    public async Task DeleteCardFromStackAsync_ShouldReturnFailure_WhenCardsEmpty()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<BaseCard>>([]));

        // Act
        var result = await _stacksService.DeleteCardFromStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.CardsNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteCardFromStackAsync_ShouldReturnFailure_WhenDeleteFails()
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

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardsDTO[0]);
        _stacksRepository.GetCardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<BaseCard>>(flashcards));
        _stacksRepository.DeleteCardFromStackAsync(flashcardId, stackId).Returns(Result.Failure(StacksErrors.DeleteCardFailed));

        // Act
        var result = await _stacksService.DeleteCardFromStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.DeleteCardFailed, result.Error);
        await _stacksRepository.Received(1).DeleteCardFromStackAsync(flashcardId, stackId);
    }

    [Fact]
    public async Task DeleteCardFromStackAsync_ShouldReturnSuccess_WhenDeleteSucceeds()
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

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardsDTO[0]);
        _stacksRepository.GetCardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<BaseCard>>(flashcards));
        _stacksRepository.DeleteCardFromStackAsync(flashcardId, stackId).Returns(Result.Success());

        // Act
        var result = await _stacksService.DeleteCardFromStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        await _stacksRepository.Received(1).DeleteCardFromStackAsync(flashcardId, stackId);
    }

    [Fact]
    public async Task UpdateCardInStackAsync_ShouldReturnFailure_WhenGetCardsByStackIdFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Failure<IEnumerable<BaseCard>>(StacksErrors.GetCardsByStackIdFailed));

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetCardsByStackIdFailed, result.Error);
    }

    [Fact]
    public async Task UpdateCardInStackAsync_ShouldReturnFailure_WhenCardsEmpty()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<BaseCard>>([]));

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.CardsNotFound, result.Error);
    }

    [Fact]
    public async Task UpdateCardInStackAsync_ShouldReturnFailure_WhenUpdateFlashcardFails()
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

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardsDTO[0]);
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _userInteractionService.GetFlashcardFront().Returns(newFront);
        _userInteractionService.GetFlashcardBack().Returns(newBack);
        _stacksRepository.GetCardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<BaseCard>>(flashcards));
        _stacksRepository.UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack)
            .Returns(Result.Failure(StacksErrors.UpdateFailed));

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.UpdateFailed, result.Error);
        await _stacksRepository.Received(1).UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack);
    }

    [Fact]
    public async Task UpdateCardInStackAsync_ShouldReturnSuccess_WhenUpdateFlashcardSucceeds()
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

        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardsDTO[0]);
        _userInteractionService.GetFlashcardFront().Returns(newFront);
        _userInteractionService.GetFlashcardBack().Returns(newBack);
        _stacksRepository.GetCardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<BaseCard>>(flashcards));
        _stacksRepository.UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack)
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        await _stacksRepository.Received(1).UpdateFlashcardInStackAsync(flashcardId, stackId, newFront, newBack);
    }

    [Fact]
    public async Task GetCardsByStackIdAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Act
        var result = await _stacksService.GetCardsByStackIdAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task GetCardsByStackIdAsync_ShouldReturnFailure_WhenRepositoryGetCardsByStackIdFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Failure<IEnumerable<BaseCard>>(StacksErrors.GetCardsByStackIdFailed));

        // Act
        var result = await _stacksService.GetCardsByStackIdAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetCardsByStackIdFailed, result.Error);
    }

    [Fact]
    public async Task GetCardsByStackIdAsync_ShouldReturnMappedCards_WhenRepositorySucceeds()
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
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<BaseCard>>(flashcards));

        // Act
        var result = await _stacksService.GetCardsByStackIdAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        foreach (var card in result.Value)
        {
            switch (card)
            {
                case FlashcardDTO flashcard:
                    Assert.Contains(result.Value, c => c is FlashcardDTO fc && fc.Id == flashcard.Id && fc.Front == flashcard.Front && fc.Back == flashcard.Back);
                    break;
                default:
                    throw new InvalidOperationException("Unknown card type");
            }
        }
    }

    [Fact]
    public async Task GetCardsCountInStackAsync_ShouldReturnFailure_WhenCurrentStackIsNull()
    {
        // Act
        var result = await _stacksService.GetCardsCountInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.CurrentStackNotFound, result.Error);
    }

    [Fact]
    public async Task GetCardsCountInStackAsync_ShouldReturnFailure_WhenRepositoryGetCardsCountFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsCountInStackAsync(stackId)
            .Returns(Result.Failure<int>(StacksErrors.GetCardsCountFailed));

        // Act
        var result = await _stacksService.GetCardsCountInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetCardsCountFailed, result.Error);
    }

    [Fact]
    public async Task GetCardsCountInStackAsync_ShouldReturnCount_WhenRepositorySucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int expectedCount = 5;
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsCountInStackAsync(stackId)
            .Returns(Result.Success(expectedCount));

        // Act
        var result = await _stacksService.GetCardsCountInStackAsync();

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
