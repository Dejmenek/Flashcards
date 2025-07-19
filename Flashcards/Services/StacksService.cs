using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Spectre.Console;

namespace Flashcards.Services;

public class StacksService : IStacksService
{
    private readonly IStacksRepository _stacksRepository;
    private readonly IFlashcardsRepository _flashcardsRepository;
    private readonly UserInteractionService _userInteractionService;
    public Stack? CurrentStack { get; private set; }

    public StacksService(IStacksRepository stacksRepository, UserInteractionService userInteractionService, IFlashcardsRepository flashcardsRepository)
    {
        _stacksRepository = stacksRepository;
        _userInteractionService = userInteractionService;
        _flashcardsRepository = flashcardsRepository;
    }

    public async Task AddStackAsync()
    {
        string name = _userInteractionService.GetStackName();

        while (await _stacksRepository.StackExistsWithNameAsync(name))
        {
            AnsiConsole.MarkupLine($"There is already a stack named {name}. Please try a different name.");
            name = _userInteractionService.GetStackName();
        }

        await _stacksRepository.AddStackAsync(name);
    }

    public async Task AddFlashcardToStackAsync()
    {
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        if (CurrentStack == null)
            return;

        await _flashcardsRepository.AddFlashcardAsync(CurrentStack.Id, front, back);
    }

    public async Task DeleteStackAsync()
    {
        if (CurrentStack == null)
            return;

        await _stacksRepository.DeleteStackAsync(CurrentStack.Id);
    }

    public async Task DeleteFlashcardFromStackAsync()
    {
        var flashcards = await GetFlashcardsByStackIdAsync();

        if (CurrentStack == null)
            return;

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);

        await _stacksRepository.DeleteFlashcardFromStackAsync(chosenFlashcard.Id, CurrentStack.Id);
    }

    public async Task UpdateFlashcardInStackAsync()
    {
        var flashcards = await GetFlashcardsByStackIdAsync();

        if (CurrentStack == null)
            return;

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        await _stacksRepository.UpdateFlashcardInStackAsync(chosenFlashcard.Id, CurrentStack.Id, front, back);
    }

    public async Task<List<FlashcardDTO>> GetFlashcardsByStackIdAsync()
    {
        if (CurrentStack == null)
            return [];

        if (!await _stacksRepository.HasStackAnyFlashcardsAsync(CurrentStack.Id))
        {
            return [];
        }

        List<FlashcardDTO> flashcardDtos = new List<FlashcardDTO>();
        var flashcards = await _stacksRepository.GetFlashcardsByStackIdAsync(CurrentStack.Id);

        foreach (var flashcard in flashcards)
        {
            flashcardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
        }

        return flashcardDtos;
    }

    public async Task<int> GetFlashcardsCountInStackAsync()
    {
        if (CurrentStack == null)
            return 0;

        return await _stacksRepository.GetFlashcardsCountInStackAsync(CurrentStack.Id);
    }

    public async Task<List<StackDTO>> GetAllStacksAsync()
    {
        if (!await _stacksRepository.HasStackAsync())
        {
            return [];
        }

        List<StackDTO> stackDtos = new List<StackDTO>();
        var stacks = await _stacksRepository.GetAllStacksAsync();

        foreach (var stack in stacks)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        return stackDtos;
    }

    public async Task GetStackAsync()
    {
        var stacks = await GetAllStacksAsync();

        if (stacks.Count == 0)
        {
            return;
        }

        string name = _userInteractionService.GetStack(stacks);

        CurrentStack = await _stacksRepository.GetStackAsync(name);
    }

    public Task<Stack> GetCurrentStackAsync()
    {
        return Task.FromResult(CurrentStack!);
    }
}
