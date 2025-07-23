using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

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

    public async Task<Result> AddFlashcardAsync()
    {
        var stacksResult = await _stacksRepository.GetAllStacksAsync();
        if (stacksResult.IsFailure) return Result.Failure(stacksResult.Error);

        if (!stacksResult.Value.Any()) return Result.Failure(StacksErrors.StacksNotFound);

        List<StackDTO> stackDtos = new();
        foreach (var stack in stacksResult.Value)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        string chosenStackName = _userInteractionService.GetStack(stackDtos);
        int chosenStackId = stacksResult.Value.Single(s => s.Name == chosenStackName).Id;
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        var addResult = await _flashcardsRepository.AddFlashcardAsync(chosenStackId, front, back);
        if (addResult.IsFailure) return Result.Failure(addResult.Error);

        return Result.Success();
    }

    public async Task<Result> DeleteFlashcardAsync()
    {
        var flashcardsResult = await GetAllFlashcardsAsync();
        if (flashcardsResult.IsFailure) return Result.Failure(flashcardsResult.Error);
        if (!flashcardsResult.Value.Any()) return Result.Failure(FlashcardsErrors.FlashcardsNotFound);

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcardsResult.Value);

        var deleteResult = await _flashcardsRepository.DeleteFlashcardAsync(chosenFlashcard.Id);
        if (deleteResult.IsFailure) return Result.Failure(deleteResult.Error);

        return Result.Success();
    }

    public async Task<Result<List<FlashcardDTO>>> GetAllFlashcardsAsync()
    {
        var flashcardsResult = await _flashcardsRepository.GetAllFlashcardsAsync();
        if (flashcardsResult.IsFailure) return Result.Failure<List<FlashcardDTO>>(flashcardsResult.Error);

        List<FlashcardDTO> flashcardDtos = new();
        foreach (var flashcard in flashcardsResult.Value)
        {
            flashcardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
        }

        return Result.Success(flashcardDtos);
    }

    public async Task<Result> UpdateFlashcardAsync()
    {
        var flashcardsResult = await GetAllFlashcardsAsync();
        if (flashcardsResult.IsFailure) return Result.Failure(flashcardsResult.Error);

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcardsResult.Value);

        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        var updateResult = await _flashcardsRepository.UpdateFlashcardAsync(chosenFlashcard.Id, front, back);
        if (updateResult.IsFailure) return Result.Failure(updateResult.Error);

        return Result.Success();
    }
}
