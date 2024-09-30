using Flashcards.DataAccess.Repositories;
using Flashcards.Helpers;
using Flashcards.Models;
using Spectre.Console;

namespace Flashcards.Services;

public class FlashcardsService
{
    private readonly FlashcardsRepository _flashcardsRepository;
    private readonly UserInteractionService _userInteractionService;
    private readonly StacksRepository _stacksRepository;

    public FlashcardsService(FlashcardsRepository flashcardsRepository, UserInteractionService userInteractionService, StacksRepository stacksRepository)
    {
        _flashcardsRepository = flashcardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
    }

    public void AddFlashcard()
    {
        var stacks = _stacksRepository.GetAllStacks();

        if (!_stacksRepository.HasStack())
        {
            AnsiConsole.MarkupLine("No stacks found. Add new stack before creating new flashcard!");
            return;
        }

        List<StackDTO> stackDtos = new List<StackDTO>();

        foreach (var stack in stacks)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        string chosenStackName = _userInteractionService.GetStack(stackDtos);

        int chosenStackId = stacks.Single(s => s.Name == chosenStackName).Id;
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        _flashcardsRepository.AddFlashcard(chosenStackId, front, back);
    }

    public void DeleteFlashcard()
    {
        List<FlashcardDTO> flashcards = GetAllFlashcards();

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);

        _flashcardsRepository.DeleteFlashcard(chosenFlashcard.Id);
    }

    public List<FlashcardDTO> GetAllFlashcards()
    {
        List<FlashcardDTO> flashcardDtos = new List<FlashcardDTO>();
        var flashcards = _flashcardsRepository.GetAllFlashcards();

        foreach (var flashcard in flashcards)
        {
            flashcardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
        }

        return flashcardDtos;
    }

    public void UpdateFlashcard()
    {
        List<FlashcardDTO> flashcards = GetAllFlashcards();

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);

        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        _flashcardsRepository.UpdateFlashcard(chosenFlashcard.Id, front, back);
    }
}
