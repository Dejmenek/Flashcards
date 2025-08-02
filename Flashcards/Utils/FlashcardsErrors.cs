namespace Flashcards.Utils;

public static class CardsErrors
{
    public static readonly Error CardsNotFound = Error.NotFound("Cards.NotFound", "No cards found for the specified stack.");
    public static readonly Error AddFailed = Error.Failure("Card.AddFailed", "Failed to add flashcard.");
    public static readonly Error DeleteFailed = Error.Failure("Card.DeleteFailed", "Failed to delete card.");
    public static readonly Error UpdateFailed = Error.Failure("Card.UpdateFailed", "Failed to update flashcard.");
    public static readonly Error GetAllFailed = Error.Failure("Card.GetAllFailed", "Failed to retrieve cards.");
}
