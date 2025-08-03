using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using NSubstitute;

namespace Flashcards.UnitTests.Services;
public class CardsServiceTests
{
    private readonly CardsService _cardsService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository _stacksRepository;
    private readonly ICardsRepository _cardsRepository;

    public CardsServiceTests()
    {
        _userInteractionService = Substitute.For<IUserInteractionService>();
        _stacksRepository = Substitute.For<IStacksRepository>();
        _cardsRepository = Substitute.For<ICardsRepository>();
        _cardsService = new CardsService(_cardsRepository, _userInteractionService, _stacksRepository);
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

        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
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

        _userInteractionService.GetStack(Arg.Any<List<StackDTO>>()).Returns("Test Stack");
        _userInteractionService.GetCardType().Returns(CardType.Flashcard);
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
        var userSelectedCard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>())
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
        var userSelectedCard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };
        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));
        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>())
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
        var expectedCards = new List<BaseCardDTO>
        {
            new FlashcardDTO { Id = 1, Front = "Front Text 1", Back = "Back Text 1" },
            new FlashcardDTO { Id = 2, Front = "Front Text 2", Back = "Back Text 2" }
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
                case FlashcardDTO flashcard:
                    Assert.Contains(result.Value, c => c is FlashcardDTO fc && fc.Id == flashcard.Id && fc.Front == flashcard.Front && fc.Back == flashcard.Back);
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
        var userSelectedCard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };

        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>())
            .Returns(userSelectedCard);
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
        var userSelectedCard = new FlashcardDTO { Id = 1, Front = "Front Text", Back = "Back Text" };

        _cardsRepository.GetAllCardsAsync()
            .Returns(Result.Success<IEnumerable<BaseCard>>(cards));

        _userInteractionService.GetCard(Arg.Any<List<BaseCardDTO>>())
            .Returns(userSelectedCard);
        _userInteractionService.GetFlashcardFront().Returns("Updated Front");
        _userInteractionService.GetFlashcardBack().Returns("Updated Back");

        _cardsRepository.UpdateFlashcardAsync(1, "Updated Front", "Updated Back")
            .Returns(Result.Success());

        // Act
        var result = await _cardsService.UpdateCardAsync();

        // Assert
        Assert.True(result.IsSuccess);
    }
}
