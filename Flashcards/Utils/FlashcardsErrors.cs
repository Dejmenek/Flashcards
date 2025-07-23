namespace Flashcards.Utils;

public static class FlashcardsErrors
{
    public static readonly Error FlashcardsNotFound = Error.NotFound("Flashcards.NotFound", "No flashcards found for the specified stack.");
    public static readonly Error AddFailed = Error.Failure("Flashcard.AddFailed", "Failed to add flashcard.");
    public static readonly Error DeleteFailed = Error.Failure("Flashcard.DeleteFailed", "Failed to delete flashcard.");
    public static readonly Error UpdateFailed = Error.Failure("Flashcard.UpdateFailed", "Failed to update flashcard.");
    public static readonly Error GetAllFailed = Error.Failure("Flashcard.GetAllFailed", "Failed to retrieve flashcards.");
}
