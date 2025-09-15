using MySql.Data.MySqlClient;
using System.Data;

public static class DatabaseMigrator
{
    public static void Migrate(string connectionString)
    {
        try
        {
            Console.WriteLine("🔄 Starting database migration...");
            
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var command = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS Vets (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    FullName VARCHAR(255) NOT NULL,
                    Email VARCHAR(255) NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Insert sample data if table is empty
                INSERT INTO Vets (FullName, Email)
                SELECT 'Dr. John Smith', 'john.smith@clinic.com'
                WHERE NOT EXISTS (SELECT 1 FROM Vets LIMIT 1);
            ", connection);

            command.ExecuteNonQuery();
            Console.WriteLine("✅ Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database migration failed: {ex.Message}");
            throw;
        }
    }
}