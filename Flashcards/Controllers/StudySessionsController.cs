using Flashcards.Models;
using Flashcards.Services.Interfaces;

namespace Flashcards.Controllers;

public class StudySessionsController
{
    private readonly IStudySessionsService _studySessionsService;

    public StudySessionsController(IStudySessionsService studySessionsService)
    {
        _studySessionsService = studySessionsService;
    }

    public async Task RunStudySessionAsync(List<FlashcardDTO> studySessionFlashcards, int stackId)
    {
        await _studySessionsService.RunStudySessionAsync(studySessionFlashcards, stackId);
    }

    public async Task<List<StudySessionDTO>> GetAllStudySessionsAsync()
    {
        return await _studySessionsService.GetAllStudySessionsAsync();
    }

    public async Task<IEnumerable<MonthlyStudySessionsNumberData>> GetMonthlyStudySessionsReportAsync()
    {
        return await _studySessionsService.GetMonthlyStudySessionsReportAsync();
    }

    public async Task<IEnumerable<MonthlyStudySessionsAverageScoreData>> GetMonthlyStudySessionsAverageScoreReportAsync()
    {
        return await _studySessionsService.GetMonthlyStudySessionsAverageScoreReportAsync();
    }
}
