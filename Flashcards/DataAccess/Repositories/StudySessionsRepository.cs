using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
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

    public async Task AddStudySessionAsync(int stackId, DateTime date, int score)
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.AddStudySession;

            await connection.ExecuteAsync(sql, new
            {
                StackId = stackId,
                Date = date,
                Score = score
            });
        }
    }

    public async Task<IEnumerable<StudySession>> GetAllStudySessionsAsync()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.GetStudySessions;

            return await connection.QueryAsync<StudySession>(sql);
        }
    }

    public async Task<IEnumerable<MonthlyStudySessionsNumberData>> GetMonthlyStudySessionReportAsync(string year)
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.GetMonthlyStudySessionReport;

            return await connection.QueryAsync<MonthlyStudySessionsNumberData>(sql, new
            {
                Year = year
            });
        }
    }

    public async Task<IEnumerable<MonthlyStudySessionsAverageScoreData>> GetMonthlyStudySessionAverageScoreReportAsync(string year)
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.GetMonthlyStudySessionAverageScoreReport;

            return await connection.QueryAsync<MonthlyStudySessionsAverageScoreData>(sql, new
            {
                Year = year
            });
        }
    }

    public async Task<bool> HasStudySessionAsync()
    {
        using (var connection = new SqlConnection(_defaultConnectionString))
        {
            string sql = SqlScripts.HasStudySession;

            return await connection.QuerySingleAsync<bool>(sql);
        }
    }
}
