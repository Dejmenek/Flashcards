using Flashcards.Enums;
using Flashcards.Models;

namespace Flashcards.Services.Interfaces;

public interface IUserInteractionService
{
    int GetId();
    string GetFlashcardFront();
    string GetFlashcardBack();
    string GetStackName();
    string GetAnswer();
    string GetYear();
    FlashcardDTO GetFlashcard(List<FlashcardDTO> flashcards);
    string GetStack(List<StackDTO> stacks);
    MenuOptions GetMenuOption();
    ManageStackOptions GetManageStackOption(string currentStack);
    ManageFlashcardsOptions GetManageFlashcardsOption();
    void GetUserInputToContinue();
}
