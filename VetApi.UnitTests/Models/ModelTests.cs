using VetApi.Models;
using Xunit;

namespace VetApi.UnitTests;

public class ModelTests
{
    [Fact]
    public void Vet_Model_CreatesSuccessfully()
    {
        // ARRANGE & ACT
        var vet = new Vet
        {
            Id = 1,
            FullName = "Dr. John Doe",
            Email = "john.doe@clinic.com"
        };

        // ASSERT
        Assert.NotNull(vet);
        Assert.Equal(1, vet.Id);
        Assert.Equal("Dr. John Doe", vet.FullName);
        Assert.Equal("john.doe@clinic.com", vet.Email);
    }

    [Fact]
    public void Vet_DefaultValues_AreEmpty()
    {
        // ARRANGE & ACT
        var vet = new Vet();

        // ASSERT
        Assert.Equal(0, vet.Id);
        Assert.Equal("", vet.FullName);
        Assert.Equal("", vet.Email);
    }
}