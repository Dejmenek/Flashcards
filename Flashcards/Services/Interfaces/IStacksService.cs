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
    Task<Result<List<BaseCardDto>>> GetCardsByStackIdAsync();
    Task<Result<List<BaseCardDto>>> GetCardsToStudyByStackIdAsync();
    Task<Result<int>> GetCardsCountInStackAsync();
    Task<Result<List<StackDto>>> GetAllStacksAsync();
    Task<Result> GetStackAsync();
    Stack GetCurrentStack();
}