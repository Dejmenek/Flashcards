using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.DataAccess.Interfaces;

public interface IStudySessionsRepository
{
    Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionReportAsync(string year);
    Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionAverageScoreReportAsync(string year);
    Task<Result> AddStudySessionAsync(int stackId, DateTime date, int score);
    Task<Result<IEnumerable<StudySession>>> GetAllStudySessionsAsync();
    Task<Result<bool>> HasStudySessionAsync();
}
