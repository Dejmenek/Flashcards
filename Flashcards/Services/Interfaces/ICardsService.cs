using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.Services.Interfaces;
public interface ICardsService
{
    Task<Result> AddCardAsync();
    Task<Result> DeleteCardAsync();
    Task<Result<List<BaseCardDTO>>> GetAllCardsAsync();
    Task<Result> UpdateCardAsync();
}