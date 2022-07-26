using PetKeeper.Core;
using System.Net;
using System.Net.Http.Json;

namespace PetKeeper.Api.Tests;

public class PetsApiTests
{
    [Fact]
    public async Task WhenAddingPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var newPet = new Pet
        {
            Name = "NewPet",
            Breed = new Breed("German Shepherd"),
            Birthday = new DateTime(2020,4,1),

        };
        var response = await client.PostAsJsonAsync(
            "https://localhost7196:/pets",
            newPet);

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdPet = await response.Content.ReadFromJsonAsync<Pet>();
        createdPet.ShouldNotBeNull();
        createdPet.Id.ShouldNotBeNullOrWhiteSpace();
        createdPet.Name.ShouldBe(newPet.Name);
        createdPet.Breed.ShouldBe(newPet.Breed);
        createdPet.Birthday.ShouldBe(newPet.Birthday);
    }

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
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pets = await response.Content.ReadFromJsonAsync<PetsResponse>();
        pets.ShouldNotBeNull();
        pets.Pets.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task WhenGettingAPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var petId = "abc123";
        var response = await client.GetAsync($"https://localhost7196:/pets/{petId}");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pet = await response.Content.ReadFromJsonAsync<Pet>();
        pet.ShouldNotBeNull();
        pet.Name.ShouldBe("Mooky");
    }

    [Fact]
    public async Task WhenAddingNeedsForUnknownPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var petId = "notapetid";
        var response = await client.PostAsJsonAsync($"https://localhost7196:/pets/{petId}/needs", new { });

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenGettingNeeds()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var petId = "abc123";
        var response = await client.GetAsync($"https://localhost7196:/pets/{petId}/needs");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var needs = await response.Content.ReadFromJsonAsync<PetNeedsResponse>();
        needs.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenGettingNeedsForUnknownPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var petId = "notapetid";
        var response = await client.GetAsync($"https://localhost7196:/pets/{petId}/needs");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenGettingAllActivities()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();

        var response = await client.GetAsync("https://localhost7196:/pets/activities");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pets = await response.Content.ReadFromJsonAsync<ActivitiesResponse>();
        pets.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenGettingActivitiesForAnUnknownPet()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        
        var petId = "notapetid";
        var response = await client.GetAsync($"https://localhost:7196/pets/{petId}/activities");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenAddingActivity()
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
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var activity = await response.Content.ReadFromJsonAsync<Activity>();
        activity.ShouldNotBeNull();
        activity.Id.ShouldNotBeNullOrWhiteSpace();
        activity.PetId.ShouldBe(petId);
        activity.Notes.ShouldBe(notes);
    }
}