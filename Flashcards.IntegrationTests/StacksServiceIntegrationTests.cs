
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
    private readonly IFlashcardsRepository _flashcardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksService _stacksService;
    private readonly IStudySessionsRepository _studySessionsRepository;

    public StacksServiceIntegrationTests(TestClassFixture fixture) : base(fixture)
    {
        _stacksRepository = _scope.ServiceProvider.GetRequiredService<IStacksRepository>();
        _flashcardsRepository = _scope.ServiceProvider.GetRequiredService<IFlashcardsRepository>();
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
    public async Task AddFlashcardToStackAsync_ShouldPersistFlashcard()
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
        var addFlashcardToStackResult = await _stacksService.AddFlashcardToStackAsync();

        // Assert
        Assert.True(addFlashcardToStackResult.IsSuccess);

        var flashcardsInStack = await _stacksRepository.GetFlashcardsByStackIdAsync(currentStack.Id);
        Assert.True(flashcardsInStack.IsSuccess);
        Assert.Contains(flashcardsInStack.Value, f => f.Front == flashcardFront && f.Back == flashcardBack);
    }

    [Fact]
    public async Task DeleteStackAsync_ShouldRemoveStack_AndCascadeDeleteRelatedFlashcardsAndStudySessions()
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

        var allFlashcards = await _flashcardsRepository.GetAllFlashcardsAsync();
        Assert.True(allFlashcards.IsSuccess);
        Assert.DoesNotContain(allFlashcards.Value, f => f.StackId == currentStack.Id);

        var allStudySessions = await _studySessionsRepository.GetAllStudySessionsAsync();
        Assert.True(allStudySessions.IsSuccess);
        Assert.DoesNotContain(allStudySessions.Value, s => s.StackId == currentStack.Id);
    }

    [Fact]
    public async Task DeleteFlashcardFromStackAsync_ShouldRemoveFlashcard()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };

        var flashcardToDelete = (await _stacksRepository.GetFlashcardsByStackIdAsync(currentStack.Id))
            .Value.First();

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>()).Returns(Mapper.ToFlashcardDTO(flashcardToDelete));

        // Act
        var deleteFlashcardFromStackResult = await _stacksService.DeleteFlashcardFromStackAsync();

        // Assert
        Assert.True(deleteFlashcardFromStackResult.IsSuccess);

        var flashcardsInStack = await _stacksRepository.GetFlashcardsByStackIdAsync(currentStack.Id);
        Assert.True(flashcardsInStack.IsSuccess);
        Assert.DoesNotContain(flashcardsInStack.Value, f => f.Id == flashcardToDelete.Id);
    }

    [Fact]
    public async Task UpdateFlashcardInStack_ShouldUpdateFlashcard()
    {
        // Arrange
        var currentStack = new Stack()
        {
            Id = 3,
            Name = "Polish",
        };
        var flashcardToUpdate = (await _stacksRepository.GetFlashcardsByStackIdAsync(currentStack.Id))
            .Value.First();
        var flashcardDTO = Mapper.ToFlashcardDTO(flashcardToUpdate);
        string updatedFront = "Updated Front";
        string updatedBack = "Updated Back";

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentStack);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>()).Returns(flashcardDTO);
        _userInteractionService.GetFlashcardFront().Returns(updatedFront);
        _userInteractionService.GetFlashcardBack().Returns(updatedBack);

        // Act
        var updateFlashcardInStackResult = await _stacksService.UpdateFlashcardInStackAsync();

        // Assert
        Assert.True(updateFlashcardInStackResult.IsSuccess);

        var flashcardsInStack = await _stacksRepository.GetFlashcardsByStackIdAsync(currentStack.Id);
        Assert.True(flashcardsInStack.IsSuccess);

        var updatedFlashcard = flashcardsInStack.Value.First(f => f.Id == flashcardToUpdate.Id);
        Assert.Equal(updatedFront, updatedFlashcard.Front);
        Assert.Equal(updatedBack, updatedFlashcard.Back);
    }

    [Fact]
    public async Task GetFlashcardsByStackId_Async_ShouldReturnFlashcardsForStack()
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
        var flashcardsInStackResult = await _stacksService.GetFlashcardsByStackIdAsync();

        // Assert
        Assert.True(flashcardsInStackResult.IsSuccess);
        Assert.NotNull(flashcardsInStackResult.Value);
    }

    [Fact]
    public async Task GetFlashcardsCountAsync_ShouldReturnCountOfFlashcardsInCurrentStack()
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
        var flashcardsCountResult = await _stacksService.GetFlashcardsCountInStackAsync();

        // Assert
        Assert.True(flashcardsCountResult.IsSuccess);
        Assert.True(flashcardsCountResult.Value >= 0);

        var flashcardsInStack = await _stacksRepository.GetFlashcardsByStackIdAsync(currentStack.Id);
        Assert.True(flashcardsInStack.IsSuccess);
        Assert.Equal(flashcardsCountResult.Value, flashcardsInStack.Value.Count());
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
