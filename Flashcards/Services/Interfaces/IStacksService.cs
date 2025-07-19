using Flashcards.Models;

namespace Flashcards.Services.Interfaces;
public interface IStacksService
{
    void AddStack();
    void AddFlashcardToStack();
    void DeleteStack();
    void DeleteFlashcardFromStack();
    void UpdateFlashcardInStack();
    List<FlashcardDTO> GetFlashcardsByStackId();
    int GetFlashcardsCountInStack();
    List<StackDTO> GetAllStacks();
    void GetStack();
    Stack GetCurrentStack();
}
