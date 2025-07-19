using Flashcards.Models;

namespace Flashcards.Services.Interfaces;
public interface IStudySessionsService
{
    Task StartStudySessionAsync(List<FlashcardDTO> flashcards);
    Task RunStudySessionAsync(List<FlashcardDTO> studySessionFlashcards, int stackId);
    Task<List<StudySessionDTO>> GetAllStudySessionsAsync();
    Task<IEnumerable<MonthlyStudySessionsNumberData>> GetMonthlyStudySessionsReportAsync();
    Task<IEnumerable<MonthlyStudySessionsAverageScoreData>> GetMonthlyStudySessionsAverageScoreReportAsync();
}
