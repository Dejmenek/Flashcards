namespace Flashcards.Utils;

public static class StacksErrors
{
    public static readonly Error StacksNotFound = Error.NotFound("Stack.StacksNotFound", "No stacks found.");
    public static readonly Error CurrentStackNotFound = Error.NotFound("Stack.CurrentStackNotFound", "No current stack selected.");
    public static readonly Error AddFailed = Error.Failure("Stack.AddFailed", "Failed to add stack.");
    public static readonly Error GetFlashcardsByStackIdFailed = Error.Failure("Stack.GetFlashcardsByStackIdFailed", "Failed to retrieve flashcards by stack ID.");
    public static readonly Error DeleteStackFailed = Error.Failure("Stack.DeleteFailed", "Failed to delete stack.");
    public static readonly Error DeleteFlashcardFailed = Error.Failure("Stack.DeleteFlashcardFailed", "Failed to delete flashcard from stack.");
    public static readonly Error GetStacksFailed = Error.Failure("Stack.GetStacksFailed", "Failed to retrieve stacks.");
    public static readonly Error GetStackFailed = Error.Failure("Stack.GetStack", "Failed to retrieve stack.");
    public static readonly Error UpdateFailed = Error.Failure("Stack.UpdateFailed", "Failed to update stack.");
    public static readonly Error StackWithNameFailed = Error.Failure("Stack.StackWithNameFailed", "Failed to check if stack with name exists.");
    public static readonly Error GetFlashcardsCountFailed = Error.Failure("Stack.GetFlashcardsCountFailed", "Failed to retrieve flashcards count for stack.");
    public static readonly Error HasAnyFlashcardsFailed = Error.Failure("Stack.HasAnyFlashcardsFailed", "Failed to check if stack has any flashcards.");
}
