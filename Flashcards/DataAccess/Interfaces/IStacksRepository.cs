using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.DataAccess.Interfaces;

public interface IStacksRepository
{
    Task<Result<Stack>> GetStackAsync(string name);
    Task<Result> DeleteStackAsync(int stackId);
    Task<Result> UpdateFlashcardInStackAsync(int flashcardId, int stackId, string front, string back);
    Task<Result> UpdateMultipleChoiceCardAsync(
        int flashcardId,
        int stackId,
        string question,
        List<string> choices,
        List<string> answer
    );
    Task<Result<int>> GetCardsCountInStackAsync(int stackId);
    Task<Result> DeleteCardFromStackAsync(int flashcardId, int stackId);
    Task<Result<IEnumerable<BaseCard>>> GetCardsByStackIdAsync(int stackId);
    Task<Result> AddStackAsync(string name);
    Task<Result<IEnumerable<Stack>>> GetAllStacksAsync();
    Task<Result<bool>> StackExistsWithNameAsync(string name);
}
