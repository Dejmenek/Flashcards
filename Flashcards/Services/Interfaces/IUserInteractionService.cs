using Flashcards.Enums;
using Flashcards.Models;

namespace Flashcards.Services.Interfaces;

public interface IUserInteractionService
{
    int GetId();
    string GetFlashcardFront();
    string GetFlashcardBack();
    string GetMultipleChoiceQuestion();
    string GetClozeDeletionText();
    List<string> GetClozeDeletionWords(string text);
    List<string> GetMultipleChoiceChoices(int numberOfChoices);
    int GetNumberOfChoices();
    List<string> GetMultipleChoiceAnswers(List<string> choices);
    string GetStackName();
    string GetAnswer();
    string GetYear();
    CardType GetCardType();
    BaseCardDto GetCard(List<BaseCardDto> cards);
    string GetStack(List<StackDto> stacks);
    string GetStack(List<StackSummaryDto> stackSummaryDtos);
    MenuOptions GetMenuOption();
    ManageStackOptions GetManageStackOption(string currentStack);
    ManageCardsOptions GetManageCardsOption();
    void GetUserInputToContinue();
}