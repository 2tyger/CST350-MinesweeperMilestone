using CST350_MinesweeperMilestone.Models;
using System.Data;
using Microsoft.Data.SqlClient;


namespace CST350_MinesweeperMilestone.Data
{
    public class UserRepository
    {
        private readonly string _connStr;

        public UserRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public bool UsernameExists(string username)
        {
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", conn);
            cmd.Parameters.AddWithValue("@Username", username);

            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        public int CreateUser(UserModel user)
        {
            const string sql = @"INSERT INTO Users (FirstName, LastName, Sex, Age, State, Email, Username, PasswordHash, Salt) 
                                OUTPUT INSERTED.Id 
                                VALUES (@FirstName, @LastName, @Sex, @Age, @State, @Email, @Username, @PasswordHash, @Salt);";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Sex", user.Sex);
            cmd.Parameters.AddWithValue("@Age", user.Age);
            cmd.Parameters.AddWithValue("@State", user.State);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

            var saltParam = new SqlParameter("@Salt", SqlDbType.VarBinary, 16) { Value = user.Salt };
            cmd.Parameters.Add(saltParam);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public UserModel? GetByUsername(string username)
        {
            const string sql = @"SELECT TOP 1 Id, FirstName, LastName, Sex, Age, State, Email, Username, PasswordHash, Salt 
                                FROM Users
                                WHERE Username = @Username;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new UserModel
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Sex = reader.GetString(3),
                Age = reader.GetInt32(4),
                State = reader.GetString(5),
                Email = reader.GetString(6),
                Username = reader.GetString(7),
                PasswordHash = reader.GetString(8),
                Salt = (byte[])reader["Salt"]
            };
        }
    }
}
