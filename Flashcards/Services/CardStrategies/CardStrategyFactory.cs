using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services.Interfaces;

namespace Flashcards.Services.CardStrategies;
public class CardStrategyFactory : ICardStrategyFactory
{
    private readonly ICardsRepository _cardsRepository;
    private readonly IUserInteractionService _userInteractionService;

    public CardStrategyFactory(ICardsRepository cardsRepository, IUserInteractionService userInteractionService)
    {
        _cardsRepository = cardsRepository;
        _userInteractionService = userInteractionService;
    }

    public ICardStrategy GetCardStrategy(CardType cardType)
    {
        return cardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService),
            CardType.MultipleChoice => new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService),
            CardType.Cloze => new ClozeCardStrategy(_cardsRepository, _userInteractionService),
            _ => throw new InvalidOperationException($"Unsupported card type: {cardType}")
        };
    }

    public ICardStrategy GetCardStrategyForStack(CardType cardType, IStacksRepository stacksRepository)
    {
        return cardType switch
        {
            CardType.Flashcard => new FlashcardStrategy(_cardsRepository, _userInteractionService, stacksRepository),
            CardType.MultipleChoice => new MultipleChoiceCardStrategy(_cardsRepository, _userInteractionService, stacksRepository),
            CardType.Cloze => new ClozeCardStrategy(_cardsRepository, _userInteractionService, stacksRepository),
            _ => throw new InvalidOperationException($"Unsupported card type: {cardType}")
        };
    }
}