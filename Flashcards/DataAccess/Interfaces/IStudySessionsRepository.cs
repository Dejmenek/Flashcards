using Flashcards.Models;

namespace Flashcards.DataAccess.Interfaces;

public interface IStudySessionsRepository
{
    Task<IEnumerable<MonthlyStudySessionsNumberData>> GetMonthlyStudySessionReportAsync(string year);
    Task<IEnumerable<MonthlyStudySessionsAverageScoreData>> GetMonthlyStudySessionAverageScoreReportAsync(string year);
    Task AddStudySessionAsync(int stackId, DateTime date, int score);
    Task<IEnumerable<StudySession>> GetAllStudySessionsAsync();
    Task<bool> HasStudySessionAsync();
}
