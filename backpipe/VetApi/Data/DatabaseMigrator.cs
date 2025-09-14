// DatabaseMigrator.cs
using MySql.Data.MySqlClient;
using System.Data;

public static class DatabaseMigrator
{
    public static void Migrate(string connectionString)
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        var command = new MySqlCommand(@"
            CREATE TABLE IF NOT EXISTS Vets (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                FullName VARCHAR(255) NOT NULL,
                Email VARCHAR(255) NOT NULL
            );
        ", connection);

        command.ExecuteNonQuery();
    }
}
