using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.Services.Interfaces;
public interface IStudySessionsService
{
    void StartStudySessionAsync(List<BaseCardDTO> cards);
    Task<Result> RunStudySessionAsync(List<BaseCardDTO> studySessionCards, int stackId);
    Task<Result<List<StudySessionDTO>>> GetAllStudySessionsAsync();
    Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionsReportAsync();
    Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionsAverageScoreReportAsync();
}