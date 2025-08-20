using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Flashcards.IntegrationTests;

public class CardsServiceIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly ICardsService _cardsService;
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository _stacksRepository;

    public CardsServiceIntegrationTests(TestClassFixture fixture) : base(fixture)
    {
        _cardsService = _scope.ServiceProvider.GetRequiredService<ICardsService>();
        _cardsRepository = _scope.ServiceProvider.GetRequiredService<ICardsRepository>();
        _userInteractionService = _scope.ServiceProvider.GetRequiredService<IUserInteractionService>();
        _stacksRepository = _scope.ServiceProvider.GetRequiredService<IStacksRepository>();
    }

    [Fact]
    public async Task AddCardAsync_ShouldPersistFlashcard()
    {
        // Arrange
        var flashcardFront = "Integration Front";
        var flashcardBack = "Integration Back";
        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        var stackDtos = stacksResult.Value.Select(Mapper.ToStackDTO).ToList();
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns(stackDtos[0].Name);
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _userInteractionService.GetFlashcardFront().Returns(flashcardFront);
        _userInteractionService.GetFlashcardBack().Returns(flashcardBack);

        // Act
        var addResult = await _cardsService.AddCardAsync();

        // Assert
        Assert.True(addResult.IsSuccess);

        var allResult = await _cardsRepository.GetAllCardsAsync();
        Assert.True(allResult.IsSuccess);
        Assert.Contains(allResult.Value, f => f is Flashcard fc && fc.Front == flashcardFront && fc.Back == flashcardBack);
    }

    [Fact]
    public async Task AddCardAsync_ShouldPersistMultipleChoiceCard()
    {
        // Arrange
        string question = "What is the capital of France?";
        var choices = new List<string> { "London", "Berlin", "Paris", "Madrid" };
        var answers = new List<string> { "Paris" };

        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        var stackDtos = stacksResult.Value.Select(Mapper.ToStackDTO).ToList();
        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns(stackDtos[0].Name);
        _userInteractionService.GetCardType().Returns(CardType.MultipleChoice);
        _userInteractionService.GetMultipleChoiceQuestion().Returns(question);
        _userInteractionService.GetNumberOfChoices().Returns(choices.Count);
        _userInteractionService.GetMultipleChoiceChoices(choices.Count).Returns(choices);
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(answers);

        // Act
        var addResult = await _cardsService.AddCardAsync();

        // Assert
        Assert.True(addResult.IsSuccess);

        var allResult = await _cardsRepository.GetAllCardsAsync();
        Assert.True(allResult.IsSuccess);
        Assert.Contains(allResult.Value,
            c => c is MultipleChoiceCard mc
                && mc.Question == question
                && mc.Choices == string.Join(";", choices)
                && mc.Answer == string.Join(";", answers)
                && mc.CardType == CardType.MultipleChoice
        );
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldUpdateFlashcard()
    {
        // Arrange
        var allFlashcards = (await _cardsRepository.GetAllCardsAsync()).Value.OfType<Flashcard>().ToList();
        var flashcard = allFlashcards.First(f => f.Front == "Hola" && f.StackId == 1);
        var flashcardDTO = Mapper.ToFlashcardDTO(flashcard);
        string updatedFront = "Updated Front";
        string updatedBack = "Updated Back";

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardDTO);
        _userInteractionService.GetFlashcardFront().Returns(updatedFront);
        _userInteractionService.GetFlashcardBack().Returns(updatedBack);

        // Act
        var updateResult = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedFlashcard = (await _cardsRepository.GetAllCardsAsync()).Value.OfType<Flashcard>().First(f => f.Id == flashcard.Id);
        Assert.Equal(updatedFront, updatedFlashcard.Front);
        Assert.Equal(updatedBack, updatedFlashcard.Back);
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldUpdateMultipleChoiceCard()
    {
        // Arrange
        var allMultipleChoiceCards = (await _cardsRepository.GetAllCardsAsync()).Value.OfType<MultipleChoiceCard>().ToList();
        var multipleChoiceCard = allMultipleChoiceCards
            .First(mc => mc.Question == "Jakie jest najwiêksze miasto w Polsce?" && mc.StackId == 3);
        var multipleChoiceCardDTO = Mapper.ToMultipleChoiceCardDTO(multipleChoiceCard);

        string updatedQuestion = "Updated question: What is the capital of France?";
        var updatedChoices = new List<string> { "London", "Berlin", "Paris", "Madrid" };
        var updatedAnswers = new List<string> { "Paris" };

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(multipleChoiceCardDTO);
        _userInteractionService.GetMultipleChoiceQuestion().Returns(updatedQuestion);
        _userInteractionService.GetNumberOfChoices().Returns(updatedChoices.Count);
        _userInteractionService.GetMultipleChoiceChoices(updatedChoices.Count).Returns(updatedChoices);
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(updatedAnswers);

        // Act
        var updateResult = await _cardsService.UpdateCardAsync();


        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedMultipleChoiceCard = (await _cardsRepository.GetAllCardsAsync()).Value.OfType<MultipleChoiceCard>()
            .First(mc => mc.Id == multipleChoiceCard.Id);

        Assert.Equal(updatedQuestion, updatedMultipleChoiceCard.Question);
        Assert.Equal(string.Join(";", updatedChoices), updatedMultipleChoiceCard.Choices);
        Assert.Equal(string.Join(";", updatedAnswers), updatedMultipleChoiceCard.Answer);
        Assert.Equal(CardType.MultipleChoice, updatedMultipleChoiceCard.CardType);
    }

    [Fact]
    public async Task DeleteCardAsync_ShouldRemoveFlashcard()
    {
        // Arrange
        var allFlashcards = (await _cardsRepository.GetAllCardsAsync()).Value.OfType<Flashcard>().ToList();
        var flashcard = allFlashcards.First(f => f.Front == "Hola" && f.StackId == 1);
        var flashcardDTO = Mapper.ToFlashcardDTO(flashcard);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>()).Returns(flashcardDTO);

        // Act
        var deleteResult = await _cardsService.DeleteCardAsync();

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var allAfterDelete = (await _cardsRepository.GetAllCardsAsync()).Value.OfType<Flashcard>().ToList();
        Assert.DoesNotContain(allAfterDelete, f => f.Id == flashcard.Id);
    }

    [Fact]
    public async Task GetAllCardsAsync_ShouldReturnCards()
    {
        // Act
        var result = await _cardsService.GetAllCardsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Contains(result.Value, f => f is FlashcardDTO fc && fc.Front == "Hola" && fc.Back == "Hello");
        Assert.Contains(result.Value, f => f is FlashcardDTO fc && fc.Front == "Hallo" && fc.Back == "Hello");
        Assert.Contains(result.Value, f => f is FlashcardDTO fc && fc.Front == "Dzieñ dobry" && fc.Back == "Good morning");
    }

    public async Task InitializeAsync() => await InitializeDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}