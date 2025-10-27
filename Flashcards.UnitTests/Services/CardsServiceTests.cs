using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.CardStrategies;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Flashcards.UnitTests.Services;
public class CardsServiceTests
{
    private readonly CardsService _cardsService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly ICardStrategyFactory _cardStrategyFactory;
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly ILogger<CardsService> _logger;

    public CardsServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _stacksRepository = Substitute.For<IStacksRepository>();
        _cardsRepository = Substitute.For<ICardsRepository>();
        _cardStrategyFactory = Substitute.For<ICardStrategyFactory>();
        _logger = Substitute.For<ILogger<CardsService>>();
        _cardsService = new CardsService(
            _cardsRepository, _userInteractionService, _stacksRepository, _cardStrategyFactory, _logger
        );
    }

    [Fact]
    public async Task AddCardAsync_ShouldReturnFailure_WhenGetAllStacksFails()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync()
            .Returns(Result.Failure<IEnumerable<Stack>>(StacksErrors.GetStacksFailed));

        // Act
        var result = await _cardsService.AddCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.GetStacksFailed, result.Error);
    }

    [Fact]
    public async Task AddCardAsync_ShouldReturnFailure_WhenNoStacksExist()
    {
        // Arrange
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success(Enumerable.Empty<Stack>()));

        // Act
        var result = await _cardsService.AddCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StacksErrors.StacksNotFound, result.Error);
    }

    [Fact]
    public async Task AddCardAsync_ShouldReturnFailure_WhenRepositoryAddFlashcardFails()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));

        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _cardStrategyFactory.GetCardStrategy(CardType.Flashcard).Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetFlashcardFront().Returns("Front Text");
        _userInteractionService.GetFlashcardBack().Returns("Back Text");

        _cardsRepository.AddFlashcardAsync(1, "Front Text", "Back Text")
            .Returns(Result.Failure(CardsErrors.AddFailed));

        // Act
        var result = await _cardsService.AddCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddCardAsync_ShouldReturnSuccess_WhenValidFlashcard()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));

        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
        _cardStrategyFactory.GetCardStrategy(CardType.Flashcard).Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetFlashcardFront().Returns("Front Text");
        _userInteractionService.GetFlashcardBack().Returns("Back Text");

        _cardsRepository.AddFlashcardAsync(1, "Front Text", "Back Text").Returns(Result.Success());

        // Act
        var result = await _cardsService.AddCardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteCardAsync_ShouldReturnFailure_WhenGetAllCardsFails()
    {
        // Arrange
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed));

        // Act
        var result = await _cardsService.DeleteCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.GetAllFailed, result.Error);
    }

    [Fact]
    public async Task DeleteCardAsync_ShouldReturnFailure_WhenNoCardsExist()
    {
        // Arrange
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success(Enumerable.Empty<BaseCard>()));

        // Act
        var result = await _cardsService.DeleteCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.CardsNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteCardAsync_ShouldReturnFailure_WhenRepositoryDeleteFails()
    {
        // Arrange
        var cards = new List<BaseCard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedCard = new FlashcardDto { Id = 1, Front = "Front Text", Back = "Back Text" };
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>())
            .Returns(userSelectedCard);
        _cardsRepository.DeleteCardAsync(1)
            .Returns(Result.Failure(CardsErrors.DeleteFailed));

        // Act
        var result = await _cardsService.DeleteCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.DeleteFailed, result.Error);
    }

    [Fact]
    public async Task DeleteCardAsync_ShouldReturnSuccess_WhenValidCardSelected()
    {
        // Arrange
        var cards = new List<BaseCard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedCard = new FlashcardDto { Id = 1, Front = "Front Text", Back = "Back Text" };
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>())
            .Returns(userSelectedCard);
        _cardsRepository.DeleteCardAsync(1).Returns(Result.Success());

        // Act
        var result = await _cardsService.DeleteCardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAllCardsAsync_ShouldReturnFailure_WhenRepositoryGetAllCardsFails()
    {
        // Arrange
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed));

        // Act
        var result = await _cardsService.GetAllCardsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.GetAllFailed, result.Error);
    }

    [Fact]
    public async Task GetAllCardsAsync_ShouldReturnMappedCards_WhenCardsExist()
    {
        // Arrange
        var cards = new List<BaseCard>
        {
            new Flashcard { Id = 1, StackId = 1, Front = "Front Text 1", Back = "Back Text 1" },
            new Flashcard { Id = 2, StackId = 2, Front = "Front Text 2", Back = "Back Text 2" }
        };

        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        var expectedCards = new List<BaseCardDto>
        {
            new FlashcardDto { Id = 1, Front = "Front Text 1", Back = "Back Text 1" },
            new FlashcardDto { Id = 2, Front = "Front Text 2", Back = "Back Text 2" }
        };

        // Act
        var result = await _cardsService.GetAllCardsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCards.Count, result.Value.Count);
        foreach (var card in expectedCards)
        {
            switch (card)
            {
                case FlashcardDto flashcard:
                    Assert.Contains(result.Value, c => c is FlashcardDto fc && fc.Id == flashcard.Id && fc.Front == flashcard.Front && fc.Back == flashcard.Back);
                    break;
                default:
                    throw new InvalidOperationException("Unknown card type");
            }
        }
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldReturnFailure_WhenGetAllCardsFails()
    {
        // Arrange
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Failure<IEnumerable<BaseCard>>(CardsErrors.GetAllFailed));

        // Act
        var result = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.GetAllFailed, result.Error);
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldReturnFailure_WhenRepositoryUpdateFails()
    {
        // Arrange
        var cards = new List<BaseCard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedCard = new FlashcardDto { Id = 1, Front = "Front Text", Back = "Back Text" };

        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>())
            .Returns(userSelectedCard);
        _cardStrategyFactory.GetCardStrategy(CardType.Flashcard).Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetFlashcardFront().Returns("Updated Front");
        _userInteractionService.GetFlashcardBack().Returns("Updated Back");

        _cardsRepository.UpdateFlashcardAsync(1, "Updated Front", "Updated Back")
            .Returns(Result.Failure(CardsErrors.UpdateFailed));

        // Act
        var result = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.UpdateFailed, result.Error);
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldReturnSuccess_WhenUpdateFlashcardSucceeds()
    {
        // Arrange
        var cards = new List<Flashcard> { new Flashcard { Id = 1, StackId = 1, Front = "Front Text", Back = "Back Text" } };
        var userSelectedCard = new FlashcardDto { Id = 1, Front = "Front Text", Back = "Back Text" };

        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>())
            .Returns(userSelectedCard);
        _cardStrategyFactory.GetCardStrategy(CardType.Flashcard).Returns(new FlashcardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetFlashcardFront().Returns("Updated Front");
        _userInteractionService.GetFlashcardBack().Returns("Updated Back");

        _cardsRepository.UpdateFlashcardAsync(1, "Updated Front", "Updated Back")
            .Returns(Result.Success());

        // Act
        var result = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddCardAsync_ShouldReturnFailure_WhenRepositoryAddMultipleChoiceCardFails()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));
        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns(CardType.MultipleChoice);
        _cardStrategyFactory.GetCardStrategy(CardType.MultipleChoice).Returns(new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService));

        _userInteractionService.GetMultipleChoiceQuestion().Returns("Question?");
        _userInteractionService.GetNumberOfChoices().Returns(2);
        _userInteractionService.GetMultipleChoiceChoices(2).Returns(new List<string> { "A", "B" });
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(new List<string> { "A" });

        _cardsRepository.AddMultipleChoiceCardAsync(1, "Question?", Arg.Any<List<string>>(), Arg.Any<List<string>>())
            .Returns(Result.Failure(CardsErrors.AddFailed));

        // Act
        var result = await _cardsService.AddCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddCardAsync_ShouldReturnFailure_WhenRepositoryAddClozeCardFails()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));

        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns(CardType.Cloze);
        _cardStrategyFactory.GetCardStrategy(CardType.Cloze).Returns(new ClozeCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetClozeDeletionText().Returns("This is a test.");
        _userInteractionService.GetClozeDeletionWords(Arg.Is("This is a test.")).Returns(new List<string> { "test" });

        _cardsRepository.AddClozeCardAsync(Arg.Is(1), Arg.Is("This is a {{c1::test}}."))
            .Returns(Result.Failure(CardsErrors.AddFailed));

        // Act
        var result = await _cardsService.AddCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.AddFailed, result.Error);
    }

    [Fact]
    public async Task AddCardAsync_ShouldThrowInvalidOperationException_WhenInvalidCardTypeSelected()
    {
        // Arrange
        var stacks = new List<Stack> { new Stack { Id = 1, Name = "Test Stack" } };
        _stacksRepository.GetAllStacksAsync().Returns(Result.Success<IEnumerable<Stack>>(stacks));
        _userInteractionService.GetStack(Arg.Any<List<StackDto>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns((CardType)999);
        _cardStrategyFactory.GetCardStrategy((CardType)999).Throws(new InvalidOperationException("Invalid card type"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _cardsService.AddCardAsync());
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldReturnFailure_WhenRepositoryUpdateMultipleChoiceCardFails()
    {
        // Arrange
        var cards = new List<BaseCard>
        {
            new MultipleChoiceCard
            {
                Id = 1,
                StackId = 1,
                Question = "Q",
                Choices = "A;B",
                Answer = "A",
                CardType = CardType.MultipleChoice
            }
        };
        var userSelectedCard = new MultipleChoiceCardDto
        {
            Id = 1,
            Question = "Q",
            Choices = new List<string> { "A", "B" },
            Answer = new List<string> { "A" },
            CardType = CardType.MultipleChoice
        };

        _cardsRepository.GetAllCardsAsync().Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(userSelectedCard);
        _cardStrategyFactory.GetCardStrategy(CardType.MultipleChoice).Returns(new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetMultipleChoiceQuestion().Returns("Updated Q");
        _userInteractionService.GetNumberOfChoices().Returns(2);
        _userInteractionService.GetMultipleChoiceChoices(2).Returns(new List<string> { "A", "B" });
        _userInteractionService.GetMultipleChoiceAnswers(Arg.Any<List<string>>()).Returns(new List<string> { "B" });

        _cardsRepository.UpdateMultipleChoiceCardAsync(1, "Updated Q", Arg.Any<List<string>>(), Arg.Any<List<string>>())
            .Returns(Result.Failure(CardsErrors.UpdateFailed));

        // Act
        var result = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.UpdateFailed, result.Error);
    }

    [Fact]
    public async Task UpdateCardAsync_ShouldReturnFailure_WhenRepositoryUpdateClozeCardFails()
    {
        // Arrange
        var cards = new List<BaseCard>
        {
            new ClozeCard
            {
                Id = 1,
                StackId = 1,
                ClozeText = "This is a {{c1::test}}.",
                CardType = CardType.Cloze
            }
        };
        var userSelectedCard = new ClozeCardDto
        {
            Id = 1,
            ClozeText = "This is a {{c1::test}}.",
            CardType = CardType.Cloze
        };

        _cardsRepository.GetAllCardsAsync().Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDto>>()).Returns(userSelectedCard);
        _cardStrategyFactory.GetCardStrategy(CardType.Cloze).Returns(new ClozeCardStrategy(_cardsRepository, _userInteractionService));
        _userInteractionService.GetClozeDeletionText().Returns("This is a updated.");
        _userInteractionService.GetClozeDeletionWords(Arg.Is("This is a updated.")).Returns(new List<string> { "updated" });

        _cardsRepository.UpdateClozeCardAsync(Arg.Is(1), Arg.Is("This is a {{c1::updated}}."))
            .Returns(Result.Failure(CardsErrors.UpdateFailed));

        // Act
        var result = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CardsErrors.UpdateFailed, result.Error);
    }
}