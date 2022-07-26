using System.Net.Http.Json;

namespace PetKeeper.Api.Tests;

public class PetsApiTests
{
    [Fact]
    public async Task WhenGettingAllPets()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();

        var response = await client.GetAsync("https://localhost7196:/pets");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var pets = await response.Content.ReadFromJsonAsync<PetsResponse>();
        pets.ShouldNotBeNull();
        pets.Pets.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task WhenAddingActivityForUnknownPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var petId = "notapetid";
        var response = await client.PostAsJsonAsync($"https://localhost7196:/pets/{petId}/activities", new { });

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenAddingActivityForKnownPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var petId = "abc123";
        var notes = "test";
        var response = await client.PostAsJsonAsync(
            $"https://localhost7196:/pets/{petId}/activities",
            new Activity
            {
                PetId = petId,
                Notes = notes
            });

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        var activity = await response.Content.ReadFromJsonAsync<Activity>();
        activity.ShouldNotBeNull();
        activity.Id.ShouldNotBeNull();
        activity.PetId.ShouldBe(petId);
        activity.Notes.ShouldBe(notes);
    }
}

public record PetsResponse
{
    public List<Pet> Pets { get; init; } = new List<Pet>();
}