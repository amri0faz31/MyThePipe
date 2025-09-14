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
            // Get connection string ONLY from IConfiguration.
            // Program.cs is responsible for providing the correct value.
            var connStr = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in configuration.");
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
