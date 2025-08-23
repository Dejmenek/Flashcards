using Flashcards.Models;
using Flashcards.Utils;

namespace Flashcards.Services.Interfaces;
public interface IStudySessionsService
{
    void StartStudySessionAsync(List<BaseCardDto> cards);
    Task<Result> RunStudySessionAsync(List<BaseCardDto> studySessionCards, int stackId);
    Task<Result<List<StudySessionDto>>> GetAllStudySessionsAsync();
    Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionsReportAsync();
    Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionsAverageScoreReportAsync();
}