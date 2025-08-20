using Dapper;

using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flashcards.DataAccess.Repositories;

public class StudySessionsRepository : IStudySessionsRepository
{
    private readonly string _defaultConnectionString;
    private readonly ILogger<StudySessionsRepository> _logger;

    public StudySessionsRepository(IConfiguration config, ILogger<StudySessionsRepository> logger)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
        _logger = logger;
    }

    public async Task<Result> AddStudySessionAsync(int stackId, DateTime date, int score)
    {
        _logger.LogInformation("Adding study session for stack {StackId} on {Date} with score {Score}.", stackId, date, score);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddStudySession;
                await connection.ExecuteAsync(sql, new { StackId = stackId, Date = date, Score = score });
            }
            _logger.LogInformation("Successfully added study session for stack {StackId} on {Date}.", stackId, date);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while adding study session for stack {StackId} on {Date}.", stackId, date);
            return Result.Failure(StudySessionsErrors.AddFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding study session for stack {StackId} on {Date}.", stackId, date);
            return Result.Failure(StudySessionsErrors.AddFailed);
        }
    }

    public async Task<Result<IEnumerable<StudySession>>> GetAllStudySessionsAsync()
    {
        _logger.LogInformation("Retrieving all study sessions.");
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetStudySessions;
                var sessions = await connection.QueryAsync<StudySession>(sql);
                _logger.LogInformation("Successfully retrieved {Count} study sessions.", sessions.Count());
                return Result.Success(sessions);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving all study sessions.");
            return Result.Failure<IEnumerable<StudySession>>(StudySessionsErrors.GetFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving all study sessions.");
            return Result.Failure<IEnumerable<StudySession>>(StudySessionsErrors.GetFailed);
        }
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionReportAsync(string year)
    {
        _logger.LogInformation("Retrieving monthly study session report for year {Year}.", year);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetMonthlyStudySessionReport;
                var report = await connection.QueryAsync<MonthlyStudySessionsNumberData>(sql, new { Year = year });
                _logger.LogInformation("Successfully retrieved monthly study session report for year {Year}.", year);
                return Result.Success(report);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving monthly study session report for year {Year}.", year);
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(StudySessionsErrors.ReportFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving monthly study session report for year {Year}.", year);
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(StudySessionsErrors.ReportFailed);
        }
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionAverageScoreReportAsync(string year)
    {
        _logger.LogInformation("Retrieving monthly average score report for year {Year}.", year);
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetMonthlyStudySessionAverageScoreReport;
                var report = await connection.QueryAsync<MonthlyStudySessionsAverageScoreData>(sql, new { Year = year });
                _logger.LogInformation("Successfully retrieved monthly average score report for year {Year}.", year);
                return Result.Success(report);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while retrieving monthly average score report for year {Year}.", year);
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(StudySessionsErrors.ReportFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving monthly average score report for year {Year}.", year);
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(StudySessionsErrors.ReportFailed);
        }
    }

    public async Task<Result<bool>> HasStudySessionAsync()
    {
        _logger.LogInformation("Checking if any study session exists.");
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.HasStudySession;
                var hasSession = await connection.QuerySingleAsync<bool>(sql);
                _logger.LogInformation("Study session exists: {HasSession}.", hasSession);
                return Result.Success(hasSession);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while checking for study session existence.");
            return Result.Failure<bool>(StudySessionsErrors.HasStudySessionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking for study session existence.");
            return Result.Failure<bool>(StudySessionsErrors.HasStudySessionFailed);
        }
    }
}