using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.Services.Interfaces;
public interface IStacksService
{
    Task<Result> AddStackAsync();
    Task<Result> AddCardToStackAsync();
    Task<Result> DeleteStackAsync();
    Task<Result> DeleteCardFromStackAsync();
    Task<Result> UpdateCardInStackAsync();
    Task<Result<List<BaseCardDTO>>> GetCardsByStackIdAsync();
    Task<Result<int>> GetCardsCountInStackAsync();
    Task<Result<List<StackDTO>>> GetAllStacksAsync();
    Task<Result> GetStackAsync();
    Stack GetCurrentStack();
}
