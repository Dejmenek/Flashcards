using Dapper;
using Flashcards.Dejmenek.DataAccess.Interfaces;
using Flashcards.Dejmenek.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace Flashcards.Dejmenek.DataAccess.Repositories;

public class StudySessionsRepository : IStudySessionsRepository
{
    private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnection"].ConnectionString;

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
