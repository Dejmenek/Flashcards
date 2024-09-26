using Flashcards.Models;
using Flashcards.Services;

namespace Flashcards.Controllers;

public class StudySessionsController
{
    private readonly StudySessionsService _studySessionsService;

    public StudySessionsController(StudySessionsService studySessionsService)
    {
        _studySessionsService = studySessionsService;
    }

    public void RunStudySession(List<FlashcardDTO> studySessionFlashcards, int stackId)
    {
        _studySessionsService.RunStudySession(studySessionFlashcards, stackId);
    }

    public List<StudySessionDTO> GetAllStudySessions()
    {
        return _studySessionsService.GetAllStudySessions();
    }

    public IEnumerable<MonthlyStudySessionsNumberData> GetMonthlyStudySessionsReport()
    {
        return _studySessionsService.GetMonthlyStudySessionsReport();
    }

    public IEnumerable<MonthlyStudySessionsAverageScoreData> GetMonthlyStudySessionsAverageScoreReport()
    {
        return _studySessionsService.GetMonthlyStudySessionsAverageScoreReport();
    }
}
