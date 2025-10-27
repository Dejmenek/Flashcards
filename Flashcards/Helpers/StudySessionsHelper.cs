namespace Flashcards.Helpers;
public static class StudySessionsHelper
{
    public static int GetNextBox(bool isCorrectAnswer, int currentBox)
    {
        return isCorrectAnswer ? Math.Min(currentBox + 1, 3) : 1;
    }

    public static DateTime GetNextReviewDate(bool isCorrectAnswer, int currentBox)
    {
        return isCorrectAnswer ? DateTime.Now.AddDays(currentBox switch
        {
            1 => 1,
            2 => 3,
            3 => 7,
            _ => 1
        }) : DateTime.Now;
    }
}
