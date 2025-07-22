using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Flashcards.Utils;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class StudySessionsRepository : IStudySessionsRepository
{
    private readonly string _defaultConnectionString;

    public StudySessionsRepository(IConfiguration config)
    {
        _defaultConnectionString = config.GetConnectionString("Default")!;
    }

    public async Task<Result> AddStudySessionAsync(int stackId, DateTime date, int score)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.AddStudySession;

                await connection.ExecuteAsync(sql, new { StackId = stackId, Date = date, Score = score });
            }
            return Result.Success();
        }
        catch (SqlException)
        {
            return Result.Failure(StudySessionsErrors.AddFailed);
        }
        catch (Exception)
        {
            return Result.Failure(StudySessionsErrors.AddFailed);
        }
    }

    public async Task<Result<IEnumerable<StudySession>>> GetAllStudySessionsAsync()
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetStudySessions;

                var sessions = await connection.QueryAsync<StudySession>(sql);
                return Result.Success(sessions);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<StudySession>>(StudySessionsErrors.GetFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<StudySession>>(StudySessionsErrors.GetFailed);
        }
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsNumberData>>> GetMonthlyStudySessionReportAsync(string year)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetMonthlyStudySessionReport;

                var report = await connection.QueryAsync<MonthlyStudySessionsNumberData>(sql, new { Year = year });
                return Result.Success(report);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(StudySessionsErrors.ReportFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<MonthlyStudySessionsNumberData>>(StudySessionsErrors.ReportFailed);
        }
    }

    public async Task<Result<IEnumerable<MonthlyStudySessionsAverageScoreData>>> GetMonthlyStudySessionAverageScoreReportAsync(string year)
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.GetMonthlyStudySessionAverageScoreReport;

                var report = await connection.QueryAsync<MonthlyStudySessionsAverageScoreData>(sql, new { Year = year });
                return Result.Success(report);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(StudySessionsErrors.ReportFailed);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<MonthlyStudySessionsAverageScoreData>>(StudySessionsErrors.ReportFailed);
        }
    }

    public async Task<Result<bool>> HasStudySessionAsync()
    {
        try
        {
            using (var connection = new SqlConnection(_defaultConnectionString))
            {
                string sql = SqlScripts.HasStudySession;

                var hasSession = await connection.QuerySingleAsync<bool>(sql);
                return Result.Success(hasSession);
            }
        }
        catch (SqlException)
        {
            return Result.Failure<bool>(StudySessionsErrors.HasStudySessionFailed);
        }
        catch (Exception)
        {
            return Result.Failure<bool>(StudySessionsErrors.HasStudySessionFailed);
        }
    }
}
