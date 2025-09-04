namespace Flashcards.Models;

public class CardProgressUpdateDto
{
    public int CardId { get; set; }
    public int Box { get; set; }
    public DateTime NextReviewDate { get; set; }
}