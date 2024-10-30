using Flashcards.Models;

namespace Flashcards.DataAccess.Interfaces;

public interface IStudySessionsRepository
{
    IEnumerable<MonthlyStudySessionsNumberData> GetMonthlyStudySessionReport(string year);
    IEnumerable<MonthlyStudySessionsAverageScoreData> GetMonthlyStudySessionAverageScoreReport(string year);
    void AddStudySession(int stackId, DateTime date, int score);
    IEnumerable<StudySession> GetAllStudySessions();
    bool HasStudySession();
}
