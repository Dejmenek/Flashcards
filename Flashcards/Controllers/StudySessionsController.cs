using Flashcards.Models;
using Flashcards.Services.Interfaces;
using Flashcards.Utils;

namespace Flashcards.Controllers;

public class StudySessionsController
{
    private readonly IStudySessionsService _studySessionsService;

    public StudySessionsController(IStudySessionsService studySessionsService)
    {
        _studySessionsService = studySessionsService;
    }

    public async Task<Result> RunStudySessionAsync(List<BaseCardDTO> studySessionCards, int stackId)
    {
        return await _studySessionsService.RunStudySessionAsync(studySessionCards, stackId);
    }

    public async Task<Result<List<StudySessionDTO>>> GetAllStudySessionsAsync()
    {
        return await _studySessionsService.GetAllStudySessionsAsync();
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionsReportAsync()
    {
        return await _studySessionsService.GetMonthlyStudySessionsReportAsync();
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionsAverageScoreReportAsync()
    {
        return await _studySessionsService.GetMonthlyStudySessionsAverageScoreReportAsync();
    }
}