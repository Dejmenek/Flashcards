namespace Flashcards.Dejmenek.Models;

public class StudySession
{
    public int Id { get; set; }
    public int StackId { get; set; }
    public DateTime Date { get; set; }
    public int Score { get; set; }
}

public record class StudySessionDTO
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public int Score { get; init; }
}
