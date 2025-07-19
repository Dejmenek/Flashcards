using Flashcards.Models;
using Flashcards.Services.Interfaces;

namespace Flashcards.Controllers;

public class StacksController
{
    private readonly IStacksService _stacksService;

    public StacksController(IStacksService stacksService)
    {
        _stacksService = stacksService;
    }

    public void AddStack()
    {
        _stacksService.AddStack();
    }

    public void AddFlashcardToStack()
    {
        _stacksService.AddFlashcardToStack();
    }

    public void DeleteStack()
    {
        _stacksService.DeleteStack();
    }

    public void DeleteFlashcardFromStack()
    {
        _stacksService.DeleteFlashcardFromStack();
    }

    public void UpdateFlashcardInStack()
    {
        _stacksService.UpdateFlashcardInStack();
    }

    public List<FlashcardDTO> GetFlashcardsByStackId()
    {
        return _stacksService.GetFlashcardsByStackId();
    }

    public int GetFlashcardsCountInStack()
    {
        return _stacksService.GetFlashcardsCountInStack();
    }

    public List<StackDTO> GetAllStacks()
    {
        return _stacksService.GetAllStacks();
    }

    public void GetStack()
    {
        _stacksService.GetStack();
    }

    public Stack GetCurrentStack()
    {
        return _stacksService.GetCurrentStack();
    }
}
