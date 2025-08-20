using Flashcards.Enums;
using Flashcards.Models;

namespace Flashcards.Services.Interfaces;

public interface IUserInteractionService
{
    int GetId();
    string GetFlashcardFront();
    string GetFlashcardBack();
    string GetMultipleChoiceQuestion();
    List<string> GetMultipleChoiceChoices(int numberOfChoices);
    int GetNumberOfChoices();
    List<string> GetMultipleChoiceAnswers(List<string> choices);
    string GetStackName();
    string GetAnswer();
    string GetYear();
    CardType GetCardType();
    BaseCardDTO GetCard(List<BaseCardDTO> cards);
    string GetStack(List<StackDTO> stacks);
    MenuOptions GetMenuOption();
    ManageStackOptions GetManageStackOption(string currentStack);
    ManageCardsOptions GetManageCardsOption();
    void GetUserInputToContinue();
}