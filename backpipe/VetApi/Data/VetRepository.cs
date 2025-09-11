using MySql.Data.MySqlClient;
using VetApi.Models;
using System;

namespace VetApi.Data
{
    public class VetRepository
    {
        private readonly string _connectionString;

        public VetRepository(IConfiguration config)
        {
            // 1. First, try to get from Environment Variable
            var connStr = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");

            // 2. If not found in environment, fall back to appsettings.json
            if (string.IsNullOrEmpty(connStr))
            {
            connStr = config.GetConnectionString("DefaultConnection");
            }

            // 3. If still not found, throw a clear error
            if (string.IsNullOrEmpty(connStr))
            {
            throw new InvalidOperationException("Database connection string not found. " +
            "Set it in appsettings.json or MYSQL_CONNECTION_STRING environment variable.");
            }

            _connectionString = connStr!;
        }
        // GET all vets
        public IEnumerable<Vet> GetAll()
        {
            var vets = new List<Vet>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("SELECT Id, FullName, Email FROM Vets", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                vets.Add(new Vet
                {
                    Id = reader.GetInt32("Id"),
                    FullName = reader.GetString("FullName"),
                    Email = reader.GetString("Email")
                });
            }
            return vets;
        }

        // INSERT new vet
        public void Add(Vet vet)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO Vets (FullName, Email) VALUES (@fullName, @email)", conn);
            cmd.Parameters.AddWithValue("@fullName", vet.FullName);
            cmd.Parameters.AddWithValue("@email", vet.Email);
            cmd.ExecuteNonQuery();
        }

        // DELETE vet
        public void Delete(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM Vets WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // UPDATE vet
        public void Update(Vet vet)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("UPDATE Vets SET FullName=@fullName, Email=@email WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", vet.Id);
            cmd.Parameters.AddWithValue("@fullName", vet.FullName);
            cmd.Parameters.AddWithValue("@email", vet.Email);
            cmd.ExecuteNonQuery();
        }
    }
}
