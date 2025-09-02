using Flashcards.Models;

namespace Flashcards.Services.CardStrategies;
public interface IStudyCardStrategy
{
    bool Study(BaseCardDto card);
}