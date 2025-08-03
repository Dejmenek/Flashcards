
using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;

namespace Flashcards.IntegrationTests;
public class StacksServiceIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksService _stacksService;
    private readonly IStudySessionsRepository _studySessionsRepository;

    public StacksServiceIntegrationTests(TestClassFixture fixture) : base(fixture)
    {
        _stacksRepository = _scope.ServiceProvider.GetRequiredService<IStacksRepository>();
        _cardsRepository = _scope.ServiceProvider.GetRequiredService<ICardsRepository>();
        _userInteractionService = _scope.ServiceProvider.GetRequiredService<IUserInteractionService>();
        _studySessionsRepository = _scope.ServiceProvider.GetRequiredService<IStudySessionsRepository>();
        _stacksService = _scope.ServiceProvider.GetRequiredService<IStacksService>();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync() => await InitializeDatabaseAsync();

    [Fact]
    public async Task AddStackAsync_ShouldPersistStack()
    {
        // Arrange
        var stackName = "English";
        _userInteractionService.GetStackName().Returns(stackName);

        // Act
        var addStackResult = await _stacksService.AddStackAsync();

        // Assert
        Assert.True(addStackResult.IsSuccess);

        var allStacks = await _stacksRepository.GetAllStacksAsync();
        Assert.True(addStackResult.IsSuccess);
        Assert.Contains(allStacks.Value, s => s.Name == stackName);
    }

    [Fact]
    public async Task AddCardToStackAsync_ShouldPersistFlashcard()
    {
        // Arrange
        string flashcardFront = "Miasto";
        string flashcardBack = "City";
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        _userInteractionService.GetFlashcardFront().Returns(flashcardFront);
        _userInteractionService.GetFlashcardBack().Returns(flashcardBack);

        // Act
        var result = await _stacksService.AddCardToStackAsync();

        // Assert
        Assert.True(result.IsSuccess);

        var cardsInStack = await _stacksRepository.GetCardsByStackIdAsync(currentStack.Id);
        Assert.True(cardsInStack.IsSuccess);
        Assert.Contains(cardsInStack.Value.OfType<Flashcard>(), f => f.Front == flashcardFront && f.Back == flashcardBack);
    }

    [Fact]
    public async Task DeleteStackAsync_ShouldRemoveStack_AndCascadeDeleteRelatedCardsAndStudySessions()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        // Act
        var deleteStackResult = await _stacksService.DeleteStackAsync();

        // Assert
        Assert.True(deleteStackResult.IsSuccess);

        var allStacks = await _stacksRepository.GetAllStacksAsync();
        Assert.True(allStacks.IsSuccess);
        Assert.DoesNotContain(allStacks.Value, s => s.Name == currentStack.Name);

        var allCards = await _cardsRepository.GetAllCardsAsync();
        Assert.True(allCards.IsSuccess);
        Assert.DoesNotContain(allCards.Value, f => f.StackId == currentStack.Id);

        var allStudySessions = await _studySessionsRepository.GetAllStudySessionsAsync();
        Assert.True(allStudySessions.IsSuccess);
        Assert.DoesNotContain(allStudySessions.Value, s => s.StackId == currentStack.Id);
    }

    [Fact]
    public async Task DeleteCardFromStackAsync_ShouldRemoveFlashcard()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        var flashcardToDelete = (await _stacksRepository.GetCardsByStackIdAsync(currentStack.Id))
            .Value.OfType<Flashcard>().First();

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(Mapper.ToFlashcardDTO(flashcardToDelete));

        // Act
        var result = await _stacksService.DeleteCardFromStackAsync();

        // Assert
        Assert.True(result.IsSuccess);

        var cardsInStack = await _stacksRepository.GetCardsByStackIdAsync(currentStack.Id);
        Assert.True(cardsInStack.IsSuccess);
        Assert.DoesNotContain(cardsInStack.Value, c => c.Id == flashcardToDelete.Id);
    }

    [Fact]
    public async Task UpdateCardInStack_ShouldUpdateFlashcard()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };
        var flashcardToUpdate = (await _stacksRepository.GetCardsByStackIdAsync(currentStack.Id))
            .Value.OfType<Flashcard>().First();
        var flashcardDTO = Mapper.ToFlashcardDTO(flashcardToUpdate);
        string updatedFront = "Updated Front";
        string updatedBack = "Updated Back";

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardDTO);
        _userInteractionService.GetFlashcardFront().Returns(updatedFront);
        _userInteractionService.GetFlashcardBack().Returns(updatedBack);

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);

        var cardsInStack = await _stacksRepository.GetCardsByStackIdAsync(currentStack.Id);
        Assert.True(cardsInStack.IsSuccess);

        var updatedFlashcard = cardsInStack.Value.OfType<Flashcard>().First(f => f.Id == flashcardToUpdate.Id);
        Assert.Equal(updatedFront, updatedFlashcard.Front);
        Assert.Equal(updatedBack, updatedFlashcard.Back);
    }

    [Fact]
    public async Task GetCardsByStackId_Async_ShouldReturnCardsForStack()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        // Act
        var result = await _stacksService.GetCardsByStackIdAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetCardsCountAsync_ShouldReturnCountOfCardsInCurrentStack()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        // Act
        var result = await _stacksService.GetCardsCountInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 0);

        var cardsInStack = await _stacksRepository.GetCardsByStackIdAsync(currentStack.Id);
        Assert.True(cardsInStack.IsSuccess);
        Assert.Equal(result.Value, cardsInStack.Value.Count());
    }

    [Fact]
    public async Task GetAllStacksAsync_ShouldReturnAllStacks()
    {
        // Act
        var allStacksResult = await _stacksService.GetAllStacksAsync();

        // Assert
        Assert.True(allStacksResult.IsSuccess);
        Assert.NotNull(allStacksResult.Value);
    }

    [Fact]
    public async Task GetStackAsync_ShouldUpdateCurrentStack()
    {
        // Arrange
        var stack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>())
            .Returns(stack.Name);

        // Act
        var getStackResult = await _stacksService.GetStackAsync();

        // Assert
        Assert.True(getStackResult.IsSuccess);
        var currentStack = typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(_stacksService) as Stack;

        Assert.NotNull(currentStack);
        Assert.Equal(stack.Id, currentStack.Id);
        Assert.Equal(stack.Name, currentStack.Name);
    }
}
