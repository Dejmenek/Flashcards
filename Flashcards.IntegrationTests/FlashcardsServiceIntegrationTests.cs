using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Flashcards.IntegrationTests;

public class FlashcardsServiceIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly IFlashcardsService _flashcardsService;
    private readonly IFlashcardsRepository _flashcardsRepository;
    private readonly IUserInteractionService _userInteractionService;

    public FlashcardsServiceIntegrationTests(TestClassFixture fixture) : base(fixture)
    {
        _flashcardsService = _scope.ServiceProvider.GetRequiredService<IFlashcardsService>();
        _flashcardsRepository = _scope.ServiceProvider.GetRequiredService<IFlashcardsRepository>();
        _userInteractionService = _scope.ServiceProvider.GetRequiredService<IUserInteractionService>();
    }

    [Fact]
    public async Task AddFlashcardService_ShouldPersistFlashcard()
    {
        // Arrange
        var flashcardFront = "Integration Front";
        var flashcardBack = "Integration Back";
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns("Spanish");
        _userInteractionService.GetFlashcardFront().Returns(flashcardFront);
        _userInteractionService.GetFlashcardBack().Returns(flashcardBack);

        // Act
        var addResult = await _flashcardsService.AddFlashcardAsync();

        // Assert
        Assert.True(addResult.IsSuccess);

        var allResult = await _flashcardsRepository.GetAllFlashcardsAsync();
        Assert.True(allResult.IsSuccess);
        Assert.Contains(allResult.Value, f => f.Front == flashcardFront && f.Back == flashcardBack);
    }

    [Fact]
    public async Task UpdateFlashcardService_ShouldUpdateFlashcard()
    {
        // Arrange
        var allFlashcards = (await _flashcardsRepository.GetAllFlashcardsAsync()).Value.ToList();
        var flashcard = allFlashcards.First(f => f.Front == "Hola" && f.StackId == 1);
        var flashcardDTO = Mapper.ToFlashcardDTO(flashcard);
        string updatedFront = "Updated Front";
        string updatedBack = "Updated Back";

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>())
            .Returns(flashcardDTO);
        _userInteractionService.GetFlashcardFront().Returns(updatedFront);
        _userInteractionService.GetFlashcardBack().Returns(updatedBack);

        // Act
        var updateResult = await _flashcardsService.UpdateFlashcardAsync();

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updated = (await _flashcardsRepository.GetAllFlashcardsAsync()).Value.First(f => f.Id == flashcard.Id);
        Assert.Equal(updatedFront, updated.Front);
        Assert.Equal(updatedBack, updated.Back);
    }

    [Fact]
    public async Task DeleteFlashcardService_ShouldRemoveFlashcard()
    {
        // Arrange
        var allFlashcards = (await _flashcardsRepository.GetAllFlashcardsAsync()).Value.ToList();
        var flashcard = allFlashcards.First(f => f.Front == "Hola" && f.StackId == 1);

        _userInteractionService.GetFlashcard(Arg.Any<List<FlashcardDTO>>())
            .Returns(Mapper.ToFlashcardDTO(flashcard));

        // Act
        var deleteResult = await _flashcardsService.DeleteFlashcardAsync();

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var allAfterDelete = (await _flashcardsRepository.GetAllFlashcardsAsync()).Value;
        Assert.DoesNotContain(allAfterDelete, f => f.Id == flashcard.Id);
    }

    [Fact]
    public async Task GetAllFlashcardsService_ShouldReturnFlashcards()
    {
        // Act
        var result = await _flashcardsService.GetAllFlashcardsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Contains(result.Value, f => f.Front == "Hola" && f.Back == "Hello");
        Assert.Contains(result.Value, f => f.Front == "Hallo" && f.Back == "Hello");
        Assert.Contains(result.Value, f => f.Front == "Dzieñ dobry" && f.Back == "Good morning");

    }

    public async Task InitializeAsync() => await InitializeDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
