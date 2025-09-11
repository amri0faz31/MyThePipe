using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;
using VetApi.Models;
using Xunit;

namespace VetApi.IntegrationTests;

// IAsyncLifetime allows us to run code before and after all tests
public class VetsControllerIntegrationTests : IAsyncLifetime
{
    private readonly MySqlContainer _dbContainer;
    private readonly HttpClient _client;

    // 1. Constructor: Sets up the TestContainer and WebApplicationFactory
    public VetsControllerIntegrationTests()
    {
        // Configure the MySQL TestContainer
        _dbContainer = new MySqlBuilder()
             .WithImage("mysql:8.0")
    .WithDatabase("vetclinic_test_db")
    .WithUsername("test_user")
    .WithPassword("test_password")
    .Build(); // Removed the WithWaitStrategy line
        // Create a custom web application factory that will override configuration
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Override configuration to use the TestContainer's connection string
                builder.ConfigureServices(services =>
                {
                    // Set the connection string as environment variable
                    Environment.SetEnvironmentVariable("MYSQL_CONNECTION_STRING", 
                        _dbContainer.GetConnectionString());
                });
            });

        _client = factory.CreateClient();
    }

    // 2. Initialize: Start the container and run migrations
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Run your database migrations on the test container
        // You'll need to make DatabaseMigrator public or extract this logic
        var connectionString = _dbContainer.GetConnectionString();
        // DatabaseMigrator.Migrate(connectionString);
        
        // For now, we'll use a simple setup script
        await InitializeDatabase();
    }

    private async Task InitializeDatabase()
    {
        // Simple SQL to create table and initial data
        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS Vets (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                FullName VARCHAR(100) NOT NULL,
                Email VARCHAR(100) NOT NULL
            );
            
            INSERT INTO Vets (FullName, Email) VALUES 
            ('Dr. Test Smith', 'test.smith@clinic.com'),
            ('Dr. Test Johnson', 'test.johnson@clinic.com');
        ";

        using var connection = new MySql.Data.MySqlClient.MySqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        
        using var command = new MySql.Data.MySqlClient.MySqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    // 3. Cleanup: Stop the container
    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        _client.Dispose();
    }

    // 4. WRITE YOUR INTEGRATION TESTS

    [Fact]
    public async Task GetAll_ReturnsListOfVets()
    {
        // Act
        var response = await _client.GetAsync("/api/vets");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var vets = await response.Content.ReadFromJsonAsync<List<Vet>>();
        Assert.NotNull(vets);
        Assert.Equal(2, vets.Count); // Because we inserted 2 in InitializeDatabase
    }

    [Fact]
    public async Task Add_ValidVet_ReturnsOkAndVetIsPersisted()
    {
        // Arrange
        var newVet = new Vet { FullName = "Dr. New Vet", Email = "new.vet@clinic.com" };
        
        // Act: Add the vet
        var addResponse = await _client.PostAsJsonAsync("/api/vets", newVet);
        addResponse.EnsureSuccessStatusCode();
        
        // Act: Get all vets to verify persistence
        var getResponse = await _client.GetAsync("/api/vets");
        var vets = await getResponse.Content.ReadFromJsonAsync<List<Vet>>();
        
        // Assert
        Assert.NotNull(vets);
        Assert.Equal(3, vets.Count); // 2 initial + 1 new
        Assert.Contains(vets, v => v.FullName == "Dr. New Vet" && v.Email == "new.vet@clinic.com");
    }

    [Fact]
    public async Task Delete_ExistingVet_RemovesFromDatabase()
    {
        // Act: Delete the first vet
        var deleteResponse = await _client.DeleteAsync("/api/vets/1");
        deleteResponse.EnsureSuccessStatusCode();
        
        // Act: Get all vets to verify deletion
        var getResponse = await _client.GetAsync("/api/vets");
        var vets = await getResponse.Content.ReadFromJsonAsync<List<Vet>>();
        
        // Assert
        Assert.NotNull(vets);
        Assert.Single(vets); // Only 1 should remain
        Assert.DoesNotContain(vets, v => v.Id == 1);
    }
}