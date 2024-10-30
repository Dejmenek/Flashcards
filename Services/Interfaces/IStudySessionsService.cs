using Flashcards.Models;

namespace Flashcards.Services.Interfaces;
public interface IStudySessionsService
{
    void StartStudySession(List<FlashcardDTO> flashcards);
    void RunStudySession(List<FlashcardDTO> studySessionFlashcards, int stackId);
    List<StudySessionDTO> GetAllStudySessions();
    IEnumerable<MonthlyStudySessionsNumberData> GetMonthlyStudySessionsReport();
    IEnumerable<MonthlyStudySessionsAverageScoreData> GetMonthlyStudySessionsAverageScoreReport();
}
