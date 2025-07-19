using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Spectre.Console;

namespace Flashcards.Services;

public class FlashcardsService : IFlashcardsService
{
    private readonly IFlashcardsRepository _flashcardsRepository;
    private readonly IStacksRepository _stacksRepository;
    private readonly UserInteractionService _userInteractionService;

    public FlashcardsService(IFlashcardsRepository flashcardsRepository, UserInteractionService userInteractionService, IStacksRepository stacksRepository)
    {
        _flashcardsRepository = flashcardsRepository;
        _userInteractionService = userInteractionService;
        _stacksRepository = stacksRepository;
    }

    public async Task AddFlashcardAsync()
    {
        var stacks = await _stacksRepository.GetAllStacksAsync();

        if (!await _stacksRepository.HasStackAsync())
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

        await _flashcardsRepository.AddFlashcardAsync(chosenStackId, front, back);
    }

    public async Task DeleteFlashcardAsync()
    {
        List<FlashcardDTO> flashcards = await GetAllFlashcardsAsync();

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);

        await _flashcardsRepository.DeleteFlashcardAsync(chosenFlashcard.Id);
    }

    public async Task<List<FlashcardDTO>> GetAllFlashcardsAsync()
    {
        List<FlashcardDTO> flashcardDtos = new List<FlashcardDTO>();
        var flashcards = await _flashcardsRepository.GetAllFlashcardsAsync();

        foreach (var flashcard in flashcards)
        {
            flashcardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
        }

        return flashcardDtos;
    }

    public async Task UpdateFlashcardAsync()
    {
        List<FlashcardDTO> flashcards = await GetAllFlashcardsAsync();

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);

        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        await _flashcardsRepository.UpdateFlashcardAsync(chosenFlashcard.Id, front, back);
    }
}
