using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;
using VetApi.Models;
using Xunit;
//using MySqlConnector; // Or MySql.Data.MySqlClient
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Hosting;              // for UseEnvironment
using Microsoft.Extensions.Configuration;       // for AddInMemoryCollection

namespace VetApi.IntegrationTests;

public class VetsControllerIntegrationTests : IAsyncLifetime
{
    private readonly MySqlContainer _dbContainer;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public VetsControllerIntegrationTests()
    {
        _dbContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("vetclinic_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // 1. Start the test container
        await _dbContainer.StartAsync();
        
        // 2. Create the factory and FORCE the environment to "Test"
        // This is the key change. It will automatically use appsettings.Test.json
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var overrides = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString()
                    };
                    config.AddInMemoryCollection(overrides);
                });
            });

        _client = _factory.CreateClient();
        
        // 3. Initialize the database using the test container's connection string
        // We use the container's string directly here for setup.
        await InitializeDatabase(_dbContainer.GetConnectionString());
    }

    private async Task InitializeDatabase(string connectionString)
    {
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

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        
        using var command = new MySqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetAll_ReturnsListOfVets()
    {
        var response = await _client.GetAsync("/api/vets");
        
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var vets = await response.Content.ReadFromJsonAsync<List<Vet>>();
        Assert.NotNull(vets);
        Assert.Equal(2, vets.Count);
    }

    [Fact]
    public async Task Add_ValidVet_ReturnsOkAndVetIsPersisted()
    {
        var newVet = new Vet { FullName = "Dr. New Vet", Email = "new.vet@clinic.com" };
        
        var addResponse = await _client.PostAsJsonAsync("/api/vets", newVet);
        addResponse.EnsureSuccessStatusCode();
        
        var getResponse = await _client.GetAsync("/api/vets");
        var vets = await getResponse.Content.ReadFromJsonAsync<List<Vet>>();
        
        Assert.NotNull(vets);
        Assert.Equal(3, vets.Count);
        Assert.Contains(vets, v => v.FullName == "Dr. New Vet" && v.Email == "new.vet@clinic.com");
    }

    [Fact]
    public async Task Delete_ExistingVet_RemovesFromDatabase()
    {
        var deleteResponse = await _client.DeleteAsync("/api/vets/1");
        deleteResponse.EnsureSuccessStatusCode();
        
        var getResponse = await _client.GetAsync("/api/vets");
        var vets = await getResponse.Content.ReadFromJsonAsync<List<Vet>>();
        
        Assert.NotNull(vets);
        Assert.Single(vets);
        Assert.DoesNotContain(vets, v => v.Id == 1);
    }
}