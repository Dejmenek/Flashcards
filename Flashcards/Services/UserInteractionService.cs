using Flashcards.Enums;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Spectre.Console;

namespace Flashcards.Services;

public class UserInteractionService : IUserInteractionService
{
    public int GetId()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>("Enter id: ")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That is not a valid id[/]")
                .Validate(Validation.IsPositiveNumber)
            );
    }

    public string GetFlashcardFront()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the front of the flashcard:")
            );
    }

    public string GetFlashcardBack()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the back of the flashcard")
            );
    }

    public string GetStackName()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the stack name")
            );
    }

    public string GetAnswer()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter your answer to this card")
            );
    }

    public string GetYear()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter year: ")
                .ValidationErrorMessage("This is not a valid year!")
                .Validate(Validation.IsValidYear)
            );
    }

    public CardType GetCardType()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<CardType>()
                .Title("Choose the type of card you want to create")
                .AddChoices(Enum.GetValues<CardType>())
            );
    }

    public BaseCardDTO GetCard(List<BaseCardDTO> cards)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<BaseCardDTO>()
                .Title("Choose your card")
                .UseConverter(card =>
                {
                    return card switch
                    {
                        FlashcardDTO flashcard => $"Flashcard: {flashcard.Front}",
                        ClozeCardDTO clozeCard => $"Cloze Card: {clozeCard.ClozeText}",
                        FillInCardDTO fillInCard => $"Fill-in Card: {fillInCard.FillInText}",
                        MultipleChoiceCardDTO multipleChoiceCard => $"Multiple Choice Card: {multipleChoiceCard.Question}",
                        _ => throw new InvalidOperationException("Unknown card type")
                    };
                })
                .AddChoices(cards)
            );
    }

    public string GetStack(List<StackDTO> stacks)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose your stack")
                .AddChoices(stacks.Select(f => f.Name))
            );
    }

    public MenuOptions GetMenuOption()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<MenuOptions>()
                .Title("What would you like to do?")
                .AddChoices(Enum.GetValues<MenuOptions>())
            );
    }

    public ManageStackOptions GetManageStackOption(string currentStack)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<ManageStackOptions>()
                .Title($"Current working stack: {currentStack}")
                .AddChoices(Enum.GetValues<ManageStackOptions>())
            );
    }

    public ManageCardsOptions GetManageCardsOption()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<ManageCardsOptions>()
                .Title($"What would you like to do with cards?")
                .AddChoices(Enum.GetValues<ManageCardsOptions>())
            );
    }

    public void GetUserInputToContinue()
    {
        AnsiConsole.MarkupLine("Press Enter to continue...");
        Console.ReadLine();
    }
}
