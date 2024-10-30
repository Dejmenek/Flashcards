using Dapper;
using Flashcards.DataAccess.Interfaces;
using Flashcards.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Flashcards.DataAccess.Repositories;

public class StudySessionsRepository : IStudySessionsRepository
{
    private readonly string _connectionString;

    public StudySessionsRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("LearnifyDb")!;
    }

    public void AddStudySession(int stackId, DateTime date, int score)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.AddStudySession;

            connection.Execute(sql, new
            {
                StackId = stackId,
                Date = date,
                Score = score
            });
        }
    }

    public IEnumerable<StudySession> GetAllStudySessions()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetStudySessions;

            return connection.Query<StudySession>(sql);
        }
    }

    public IEnumerable<MonthlyStudySessionsNumberData> GetMonthlyStudySessionReport(string year)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetMonthlyStudySessionReport;

            return connection.Query<MonthlyStudySessionsNumberData>(sql, new
            {
                Year = year
            });
        }
    }

    public IEnumerable<MonthlyStudySessionsAverageScoreData> GetMonthlyStudySessionAverageScoreReport(string year)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.GetMonthlyStudySessionAverageScoreReport;

            return connection.Query<MonthlyStudySessionsAverageScoreData>(sql, new
            {
                Year = year
            });
        }
    }

    public bool HasStudySession()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = SqlScripts.HasStudySession;

            return connection.QuerySingle<bool>(sql);
        }
    }
}
