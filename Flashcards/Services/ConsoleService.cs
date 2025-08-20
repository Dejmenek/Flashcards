using Flashcards.Services.Interfaces;

namespace Flashcards.Services;

public class ConsoleService : IConsoleService
{
    public void Clear()
    {
        Console.Clear();
    }
}