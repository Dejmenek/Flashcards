﻿using Flashcards.DataAccess.Repositories;
using Flashcards.Helpers;
using Flashcards.Models;
using Spectre.Console;

namespace Flashcards.Services;

public class StacksService
{
    private readonly StacksRepository _stacksRepository;
    private readonly UserInteractionService _userInteractionService;
    private readonly FlashcardsRepository _flashcardsRepository;
    public Stack? CurrentStack { get; private set; }

    public StacksService(StacksRepository stacksRepository, UserInteractionService userInteractionService, FlashcardsRepository flashcardsRepository)
    {
        _stacksRepository = stacksRepository;
        _userInteractionService = userInteractionService;
        _flashcardsRepository = flashcardsRepository;
    }

    public void AddStack()
    {
        string name = _userInteractionService.GetStackName();

        while (_stacksRepository.StackExistsWithName(name))
        {
            AnsiConsole.MarkupLine($"There is already a stack named {name}. Please try a different name.");
            name = _userInteractionService.GetStackName();
        }

        _stacksRepository.AddStack(name);
    }

    public void AddFlashcardToStack()
    {
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        _flashcardsRepository.AddFlashcard(CurrentStack.Id, front, back);
    }

    public void DeleteStack()
    {
        _stacksRepository.DeleteStack(CurrentStack.Id);
    }

    public void DeleteFlashcardFromStack()
    {
        List<FlashcardDTO> flashcards = GetFlashcardsByStackId();

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);

        _stacksRepository.DeleteFlashcardFromStack(chosenFlashcard.Id, CurrentStack.Id);
    }

    public void UpdateFlashcardInStack()
    {
        List<FlashcardDTO> flashcards = GetFlashcardsByStackId();

        FlashcardDTO chosenFlashcard = _userInteractionService.GetFlashcard(flashcards);
        string front = _userInteractionService.GetFlashcardFront();
        string back = _userInteractionService.GetFlashcardBack();

        _stacksRepository.UpdateFlashcardInStack(chosenFlashcard.Id, CurrentStack.Id, front, back);
    }

    public List<FlashcardDTO> GetFlashcardsByStackId()
    {
        if (!_stacksRepository.HasStackAnyFlashcards(CurrentStack.Id))
        {
            return [];
        }

        List<FlashcardDTO> flashcardDtos = new List<FlashcardDTO>();
        var flashcards = _stacksRepository.GetFlashcardsByStackId(CurrentStack.Id);

        foreach (var flashcard in flashcards)
        {
            flashcardDtos.Add(Mapper.ToFlashcardDTO(flashcard));
        }

        return flashcardDtos;
    }

    public int GetFlashcardsCountInStack()
    {
        return _stacksRepository.GetFlashcardsCountInStack(CurrentStack.Id);
    }

    public List<StackDTO> GetAllStacks()
    {
        if (!_stacksRepository.HasStack())
        {
            return [];
        }

        List<StackDTO> stackDtos = new List<StackDTO>();
        var stacks = _stacksRepository.GetAllStacks();

        foreach (var stack in stacks)
        {
            stackDtos.Add(Mapper.ToStackDTO(stack));
        }

        return stackDtos;
    }

    public void GetStack()
    {
        string name = _userInteractionService.GetStack(GetAllStacks());

        CurrentStack = _stacksRepository.GetStack(name);
    }
}
