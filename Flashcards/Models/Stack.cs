namespace Flashcards.Models;

public class Stack
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Flashcard> Flashcards { get; set; }
}

public record class StackDto
{
    public string Name { get; init; }
}

public record class StackSummaryDto
{
    public string Name { get; init; }
    public int TotalCards { get; init; }
    public int DueCards { get; init; }
}