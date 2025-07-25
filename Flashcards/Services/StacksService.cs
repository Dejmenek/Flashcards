using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;
using Spectre.Console;

namespace Flashcards.Services;

public class StacksService : IStacksService
{
    private readonly IStacksRepository _stacksRepository;
    private readonly IFlashcardsRepository _flashcardsRepository;
    private readonly IUserInteractionService _userInteractionService;
    public Stack? CurrentStack { get; private set; }

    public StacksService(IStacksRepository stacksRepository, IUserInteractionService userInteractionService, IFlashcardsRepository flashcardsRepository)
    {
        _stacksRepository = stacksRepository;
        _userInteractionService = userInteractionService;
        _flashcardsRepository = flashcardsRepository;
    }

    public async Task<Result> AddStackAsync()
    {
        string name = _userInteractionService.GetStackName();

        while (true)
        {
            var existsResult = await _stacksRepository.StackExistsWithNameAsync(name);
            if (existsResult.IsFailure) return Result.Failure(existsResult.Error);
            if (!existsResult.Value) break;

            AnsiConsole.MarkupLine($"There is already a stack named {name}. Please try a different name.");
            name = _userInteractionService.GetStackName();
        }

        var addResult = await _stacksRepository.AddStackAsync(name);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> AddFlashcardToStackAsync()
    {
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        if (CurrentStack == null)
            return Result.Failure(StacksErrors.CurrentStackNotFound);

        var addResult = await _flashcardsRepository.AddFlashcardAsync(CurrentStack.Id, front, back);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> DeleteStackAsync()
    {
        if (CurrentStack == null)
            return Result.Failure(StacksErrors.CurrentStackNotFound);

        var deleteResult = await _stacksRepository.DeleteStackAsync(CurrentStack.Id);
        if (deleteResult.IsFailure) return Result.Failure(deleteResult.Error);

        return Result.Success();
    }

    public async Task<Result> DeleteFlashcardFromStackAsync()
    {
        var flashcardsResult = await GetFlashcardsByStackIdAsync();
        if (flashcardsResult.IsFailure) return Result.Failure(flashcardsResult.Error);

        if (flashcardsResult.Value.Count == 0)
            return Result.Failure(FlashcardsErrors.FlashcardsNotFound);

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcardsResult.Value);

        var deleteResult = await _stacksRepository.DeleteFlashcardFromStackAsync(chosenFlashcard.Id, CurrentStack.Id);
        if (deleteResult.IsFailure) return Result.Failure(deleteResult.Error);

        return Result.Success();
    }

    public async Task<Result> UpdateFlashcardInStackAsync()
    {
        var flashcardsResult = await GetFlashcardsByStackIdAsync();
        if (flashcardsResult.IsFailure) return Result.Failure(flashcardsResult.Error);

        if (flashcardsResult.Value.Count == 0)
            return Result.Failure(FlashcardsErrors.FlashcardsNotFound);

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcardsResult.Value);
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        var updateResult = await _stacksRepository.UpdateFlashcardInStackAsync(chosenFlashcard.Id, CurrentStack.Id, front, back);
        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }

    public async Task<Result<List<FlashcardDTO>>> GetFlashcardsByStackIdAsync()
    {
        if (CurrentStack == null)
            return Result.Failure<List<FlashcardDTO>>(StacksErrors.CurrentStackNotFound);

        var flashcardsResult = await _stacksRepository.GetFlashcardsByStackIdAsync(CurrentStack.Id);
        if (flashcardsResult.IsFailure) return Result.Failure<List<FlashcardDTO>>(flashcardsResult.Error);

        List<FlashcardDTO> flashcardDtos = new();
        foreach (var flashcard in flashcardsResult.Value)
        {
            flashcardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
        }

        return Result.Success(flashcardDtos);
    }

    public async Task<Result<int>> GetFlashcardsCountInStackAsync()
    {
        if (CurrentStack == null)
            return Result.Failure<int>(StacksErrors.CurrentStackNotFound);

        var countResult = await _stacksRepository.GetFlashcardsCountInStackAsync(CurrentStack.Id);
        if (countResult.IsFailure) return Result.Failure<int>(countResult.Error);

        return Result.Success(countResult.Value);
    }

    public async Task<Result<List<StackDTO>>> GetAllStacksAsync()
    {
        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        if (stacksResult.IsFailure) return Result.Failure<List<StackDTO>>(stacksResult.Error);

        List<StackDTO> stackDtos = new();
        foreach (var stack in stacksResult.Value)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        return Result.Success(stackDtos);
    }

    public async Task<Result> GetStackAsync()
    {
        var stacksResult = await GetAllStacksAsync();
        if (stacksResult.IsFailure) return Result.Failure(stacksResult.Error);

        if (stacksResult.Value.Count == 0)
            return Result.Failure(StacksErrors.StacksNotFound);

        string name = _userInteractionService.GetStack(stacksResult.Value);

        var stackResult = await _stacksRepository.GetStackAsync(name);
        if (stackResult.IsFailure) return Result.Failure(stackResult.Error);

        CurrentStack = stackResult.Value;
        return Result.Success();
    }

    public Stack GetCurrentStack()
    {
        return CurrentStack!;
    }
}
