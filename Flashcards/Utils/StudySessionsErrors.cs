namespace Flashcards.Utils;

public static class StudySessionsErrors
{
    public static readonly Error HasStudySessionFailed = Error.Failure("StudySession.HasStudySessionFailed", "Failed to check if study session exists.");
    public static readonly Error AddFailed = Error.Failure("StudySession.AddFailed", "Failed to add study session.");
    public static readonly Error GetFailed = Error.Failure("StudySession.GetFailed", "Failed to retrieve study sessions.");
    public static readonly Error ReportFailed = Error.Failure("StudySession.ReportFailed", "Failed to generate study session report.");
}
