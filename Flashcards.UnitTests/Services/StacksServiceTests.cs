using System.Reflection;

using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Flashcards.UnitTests.Services;
public class StacksServiceTests
{
    private readonly StacksService _stacksService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly ICardStrategyFactory _cardStrategyFactory;
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly ILogger<StacksService> _logger;

    public StacksServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _stacksRepository = Substitute.For<IStacksRepository>();
        _cardsRepository = Substitute.For<ICardsRepository>();
        _cardStrategyFactory = Substitute.For<ICardStrategyFactory>();
        _logger = Substitute.For<ILogger<StacksService>>();
        _stacksService = new StacksService(
            _stacksRepository, _userInteractionService, _cardStrategyFactory, _logger
        );
    }

    private Stack CreateTestStack(int id = 1, string name = "Test Stack") =>
        new() { Id = id, Name = name };

    private Flashcard CreateTestFlashcard(int id = 1, int stackId = 1, string front = "Front", string back = "Back") =>
        new() { Id = id, StackId = stackId, Front = front, Back = back };

    private FlashcardDto CreateTestFlashcardDTO(int id = 1, string front = "Front", string back = "Back") =>
        new() { Id = id, Front = front, Back = back };

    private MultipleChoiceCard CreateTestMultipleChoiceCard(int id = 1, int stackId = 1, string question = "Question?", string choices = "A;B;C", string answer = "A") =>
    new() { Id = id, StackId = stackId, Question = question, Choices = choices, Answer = answer };

    private ClozeCard CreateTestClozeCard(int id = 1, int stackId = 1, string clozeText = "Cloze text") =>
        new() { Id = id, StackId = stackId, ClozeText = clozeText };

    private ClozeCardDto CreateTestClozeCardDTO(int id = 1, string clozeText = "Cloze text") =>
        new() { Id = id, ClozeText = clozeText, CardType = CardType.Cloze };

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
        _cardStrategyFactory.GetCardStrategy(CardType.Flashcard)
            .Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService));
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
    public async Task AddCardToStackAsync_ShouldReturnFailure_WhenAddMultipleChoiceCardFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCardType().Returns(CardType.MultipleChoice);
        _cardStrategyFactory.GetCardStrategy(CardType.MultipleChoice)
            .Returns(new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetMultipleChoiceQuestion().Returns("Question?");
        _userInteractionService.GetNumberOfChoices().Returns(2);
        _userInteractionService.GetMultipleChoiceChoices(2).Returns(new List<string> { "Choice A", "Choice B" });
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(new List<string> { "Choice A" });

        _cardsRepository.AddMultipleChoiceCardAsync(stackId, "Question?", Arg.Any<List<string>>(), Arg.Any<List<string>>())
            .Returns(Result.Failure(CardsErrors.AddFailed));

        // Act
        var result = await _stacksService.AddCardToStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddCardToStackAsync_ShouldReturnFailure_WhenAddClozeCardFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        string clozeText = "Cloze text";
        string formattedClozeText = "Cloze {{c1::text}}";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCardType().Returns(CardType.Cloze);
        _cardStrategyFactory.GetCardStrategy(CardType.Cloze)
            .Returns(new ClozeCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetClozeDeletionText().Returns(clozeText);
        _userInteractionService.GetClozeDeletionWords(Arg.Is(clozeText)).Returns(new List<string> { "text" });

        _cardsRepository.AddClozeCardAsync(Arg.Is(stackId), Arg.Is(formattedClozeText))
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
        _cardStrategyFactory.GetCardStrategy(CardType.Flashcard)
            .Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService));
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
    public async Task AddCardToStackAsync_ShouldReturnSuccess_WhenAddMultipleChoiceCardSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCardType().Returns(CardType.MultipleChoice);
        _cardStrategyFactory.GetCardStrategy(CardType.MultipleChoice)
            .Returns(new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetMultipleChoiceQuestion().Returns("Question?");
        _userInteractionService.GetNumberOfChoices().Returns(2);
        _userInteractionService.GetMultipleChoiceChoices(2).Returns(new List<string> { "Choice A", "Choice B" });
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(new List<string> { "Choice A" });

        _cardsRepository.AddMultipleChoiceCardAsync(stackId, "Question?", Arg.Any<List<string>>(), Arg.Any<List<string>>())
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.AddCardToStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddCardToStackAsync_ShouldReturnSuccess_WhenAddClozeCardSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        string clozeText = "Cloze text";
        string formattedClozeText = "Cloze {{c1::text}}";
        var currentTestStack = CreateTestStack(stackId, stackName);
        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCardType().Returns(CardType.Cloze);
        _cardStrategyFactory.GetCardStrategy(CardType.Cloze)
            .Returns(new ClozeCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetClozeDeletionText().Returns(clozeText);
        _userInteractionService.GetClozeDeletionWords(Arg.Is(clozeText)).Returns(new List<string> { "text" });

        _cardsRepository.AddClozeCardAsync(Arg.Is(stackId), Arg.Is(formattedClozeText))
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
        var flashcardsDTO = new List<FlashcardDto> { CreateTestFlashcardDTO(flashcardId, front, back) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(flashcardsDTO[0]);
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
        var flashcardsDTO = new List<FlashcardDto> { CreateTestFlashcardDTO(flashcardId, front, back) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(flashcardsDTO[0]);
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
        var flashcardsDTO = new List<FlashcardDto> { CreateTestFlashcardDTO(flashcardId, originalFront, originalBack) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(flashcardsDTO[0]);
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _cardStrategyFactory.GetCardStrategyForStack(CardType.Flashcard, _stacksRepository)
            .Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService, _stacksRepository));
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
    public async Task UpdateCardInStackAsync_ShouldReturnFailure_WhenUpdateMultipleChoiceCardFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int cardId = 1;
        var currentTestStack = CreateTestStack(stackId, stackName);
        var multipleChoiceCard = new MultipleChoiceCard
        {
            Id = cardId,
            StackId = stackId,
            Question = "Original Question",
            Choices = "A;B;C",
            Answer = "A"
        };
        var multipleChoiceCardDTO = new MultipleChoiceCardDto
        {
            Id = cardId,
            Question = "Original Question",
            Choices = new List<string> { "A", "B", "C" },
            Answer = new List<string> { "A" },
            CardType = CardType.MultipleChoice
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(multipleChoiceCardDTO);
        _cardStrategyFactory.GetCardStrategyForStack(CardType.MultipleChoice, _stacksRepository)
            .Returns(new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService, _stacksRepository));
        _userInteractionService.GetMultipleChoiceQuestion().Returns("Updated Question");
        _userInteractionService.GetNumberOfChoices().Returns(3);
        _userInteractionService.GetMultipleChoiceChoices(3).Returns(new List<string> { "A", "B", "C" });
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(new List<string> { "B" });

        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<BaseCard>>(new List<BaseCard> { multipleChoiceCard }));
        _stacksRepository.UpdateMultipleChoiceCardAsync(cardId, stackId, "Updated Question", Arg.Any<List<string>>(), Arg.Any<List<string>>())
            .Returns(Result.Failure(StacksErrors.UpdateFailed));

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.UpdateFailed, result.Error);
    }

    [Fact]
    public async Task UpdateCardInStackAsync_ShouldReturnFailure_WhenUpdateClozeCardFails()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int clozeCardId = 1;
        string originalText = "Original Cloze";
        string newText = "Updated cloze";
        string formattedNewText = "Updated {{c1::cloze}}";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var clozeCards = new List<ClozeCard> { CreateTestClozeCard(clozeCardId, stackId, originalText) };
        var clozeCardsDTO = new List<ClozeCardDto> { CreateTestClozeCardDTO(clozeCardId, originalText) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(clozeCardsDTO[0]);
        _userInteractionService.GetCardType().Returns(CardType.Cloze);
        _cardStrategyFactory.GetCardStrategyForStack(CardType.Cloze, _stacksRepository)
            .Returns(new ClozeCardStrategy(_cardsRepository, _userInteractionService, _stacksRepository));
        _userInteractionService.GetClozeDeletionText().Returns(newText);
        _userInteractionService.GetClozeDeletionWords(Arg.Is(newText)).Returns(new List<string> { "cloze" });
        _stacksRepository.GetCardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<BaseCard>>(clozeCards));
        _stacksRepository.UpdateClozeCardInStackAsync(clozeCardId, stackId, Arg.Is(formattedNewText))
            .Returns(Result.Failure(StacksErrors.UpdateFailed));

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.UpdateFailed, result.Error);
        await _stacksRepository.Received(1).UpdateClozeCardInStackAsync(clozeCardId, stackId, Arg.Is(formattedNewText));
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
        var flashcardsDTO = new List<FlashcardDto> { CreateTestFlashcardDTO(flashcardId, originalFront, originalBack) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _cardStrategyFactory.GetCardStrategyForStack(CardType.Flashcard, _stacksRepository)
            .Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService, _stacksRepository));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(flashcardsDTO[0]);
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
    public async Task UpdateCardInStackAsync_ShouldReturnSuccess_WhenUpdateMultipleChoiceCardSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int cardId = 1;
        var currentTestStack = CreateTestStack(stackId, stackName);
        var multipleChoiceCard = new MultipleChoiceCard
        {
            Id = cardId,
            StackId = stackId,
            Question = "Original Question",
            Choices = "A;B;C",
            Answer = "A"
        };
        var multipleChoiceCardDTO = new MultipleChoiceCardDto
        {
            Id = cardId,
            Question = "Original Question",
            Choices = new List<string> { "A", "B", "C" },
            Answer = new List<string> { "A" },
            CardType = CardType.MultipleChoice
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(multipleChoiceCardDTO);
        _cardStrategyFactory.GetCardStrategyForStack(CardType.MultipleChoice, _stacksRepository)
            .Returns(new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService, _stacksRepository));
        _userInteractionService.GetMultipleChoiceQuestion().Returns("Updated Question");
        _userInteractionService.GetNumberOfChoices().Returns(3);
        _userInteractionService.GetMultipleChoiceChoices(3).Returns(new List<string> { "A", "B", "C" });
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(new List<string> { "B" });

        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<BaseCard>>(new List<BaseCard> { multipleChoiceCard }));
        _stacksRepository.UpdateMultipleChoiceCardAsync(cardId, stackId, "Updated Question", Arg.Any<List<string>>(), Arg.Any<List<string>>())
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateCardInStackAsync_ShouldReturnSuccess_WhenUpdateClozeCardSucceeds()
    {
        // Arrange
        int stackId = 1;
        string stackName = "Test Stack";
        int clozeCardId = 1;
        string originalText = "Original Cloze";
        string newText = "Updated cloze";
        string formattedNewText = "Updated {{c1::cloze}}";
        var currentTestStack = CreateTestStack(stackId, stackName);
        var clozeCards = new List<ClozeCard> { CreateTestClozeCard(clozeCardId, stackId, originalText) };
        var clozeCardsDTO = new List<ClozeCardDto> { CreateTestClozeCardDTO(clozeCardId, originalText) };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);

        _userInteractionService.GetCardType().Returns(CardType.Cloze);
        _cardStrategyFactory.GetCardStrategyForStack(CardType.Cloze, _stacksRepository)
            .Returns(new ClozeCardStrategy(_cardsRepository, _userInteractionService, _stacksRepository));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(clozeCardsDTO[0]);
        _userInteractionService.GetClozeDeletionText().Returns(newText);
        _userInteractionService.GetClozeDeletionWords(Arg.Is(newText)).Returns(new List<string> { "cloze" });
        _stacksRepository.GetCardsByStackIdAsync(stackId).Returns(Result.Success<IEnumerable<BaseCard>>(clozeCards));
        _stacksRepository.UpdateClozeCardInStackAsync(clozeCardId, stackId, Arg.Is(formattedNewText))
            .Returns(Result.Success());

        // Act
        var result = await _stacksService.UpdateCardInStackAsync();

        // Assert
        Assert.True(result.IsSuccess);
        await _stacksRepository.Received(1).UpdateClozeCardInStackAsync(clozeCardId, stackId, Arg.Is(formattedNewText));
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
        var cards = new List<BaseCard>
        {
            CreateTestFlashcard(1, stackId, "Front1", "Back1"),
            CreateTestFlashcard(2, stackId, "Front2", "Back2"),
            CreateTestMultipleChoiceCard(3, stackId, "Question?", "A;B;C", "A"),
            CreateTestMultipleChoiceCard(4, stackId, "Another Question?", "D;E;F", "D"),
            CreateTestClozeCard(5, stackId, "Cloze1"),
            CreateTestClozeCard(6, stackId, "Cloze2")
        };

        typeof(StacksService)
            .GetProperty("CurrentStack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(_stacksService, currentTestStack);
        _stacksRepository.GetCardsByStackIdAsync(stackId)
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));

        // Act
        var result = await _stacksService.GetCardsByStackIdAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(cards.Count, result.Value.Count);
        foreach (var card in result.Value)
        {
            switch (card)
            {
                case FlashcardDto flashcard:
                    Assert.Contains(result.Value, c => c is FlashcardDto fc && fc.Id == flashcard.Id && fc.Front == flashcard.Front && fc.Back == flashcard.Back);
                    break;
                case MultipleChoiceCardDto multipleChoiceCard:
                    Assert.Contains(result.Value, c => c is MultipleChoiceCardDto mcc && mcc.Id == multipleChoiceCard.Id && mcc.Question == multipleChoiceCard.Question && mcc.Choices.SequenceEqual(multipleChoiceCard.Choices) && mcc.Answer.SequenceEqual(multipleChoiceCard.Answer));
                    break;
                case ClozeCardDto clozeCard:
                    Assert.Contains(result.Value, c => c is ClozeCardDto cc && cc.Id == clozeCard.Id && cc.ClozeText == clozeCard.ClozeText);
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
        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns(stackName);
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
        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns(stackName);
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