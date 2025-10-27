using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Services.CardStrategies;

namespace Flashcards.Services.Interfaces;
public interface ICardStrategyFactory
{
    ICardStrategy GetCardStrategy(CardType cardType);
    ICardStrategy GetCardStrategyForStack(CardType cardType, IStacksRepository stacksRepository);
}