using CST350_MinesweeperMilestone.Models;
using Microsoft.Data.SqlClient;

namespace CST350_MinesweeperMilestone.Data
{
    public class GameRepository
    {
        private readonly string _connStr;

        public GameRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public int SaveGame(int userId, string gameData, DateTime dateSaved)
        {
            const string sql = @"INSERT INTO Games (UserId, DateSaved, GameData)
                                OUTPUT INSERTED.Id
                                VALUES (@UserId, @DateSaved, @GameData);";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@DateSaved", dateSaved);
            cmd.Parameters.AddWithValue("@GameData", gameData);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public List<SavedGameSummary> GetAllGames()
        {
            const string sql = @"SELECT Id, UserId, DateSaved
                                FROM Games
                                ORDER BY DateSaved DESC;";

            var results = new List<SavedGameSummary>();
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new SavedGameSummary
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    DateSaved = reader.GetDateTime(2)
                });
            }

            return results;
        }

        public SavedGame? GetGameById(int id)
        {
            const string sql = @"SELECT Id, UserId, DateSaved, GameData
                                FROM Games
                                WHERE Id = @Id;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new SavedGame
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                DateSaved = reader.GetDateTime(2),
                GameData = reader.GetString(3)
            };
        }

        public bool DeleteGame(int id)
        {
            const string sql = @"DELETE FROM Games WHERE Id = @Id;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
