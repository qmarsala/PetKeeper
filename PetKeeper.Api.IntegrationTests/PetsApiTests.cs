using PetKeeper.Api.Responses;
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
        var response = await client.PostAsJsonAsync("/pets", newPet);

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
        var pet = InsertPet(client, "NewPetForGettingAll");
        
        var response = await client.GetAsync("/pets");
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

        var newPet = await InsertPet(client, "NewPetForGettingAPetById");
        var response = await client.GetAsync($"/pets/{newPet.Id}");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pet = await response.Content.ReadFromJsonAsync<Pet>();
        pet.ShouldNotBeNull();
        pet.Name.ShouldBe(newPet.Name);
    }

    [Fact]
    public async Task WhenAddingNeeds()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {

            });

        var client = application.CreateClient();
        var pet = await InsertPet(client, "NewPetForAddingNeeds");
        var newNeed = new Need
        {
            Name = "Tests",
            Days = new List<DayOfWeek>() { { DayOfWeek.Monday } },
            Times = 3
        };
        var response = await client.PostAsJsonAsync($"/pets/{pet.Id}/needs", newNeed);

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdNeed = await response.Content.ReadFromJsonAsync<Need>();
        createdNeed.ShouldNotBeNull();
        createdNeed.Id.ShouldNotBeNullOrWhiteSpace();
        createdNeed.Name.ShouldBe(newNeed.Name);
        createdNeed.Days.ShouldBe(newNeed.Days);
        createdNeed.Times.ShouldBe(newNeed.Times);
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
        var response = await client.PostAsJsonAsync($"/pets/{petId}/needs", new { });

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
        var pet = await InsertPet(client, "NewPetForGettingNeeds", new Need
        {
            Name = "Tests",
            Days = new List<DayOfWeek>() { { DayOfWeek.Monday } },
            Times = 3
        });

        var response = await client.GetAsync($"/pets/{pet.Id}/needs");

        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var needs = await response.Content.ReadFromJsonAsync<PetNeedsResponse>();
        needs.ShouldNotBeNull();
        needs.PetNeeds.Count.ShouldBeGreaterThan(0);
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
        var response = await client.GetAsync($"/pets/{petId}/needs");

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

        var response = await client.GetAsync("/pets/activities");

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
        var response = await client.GetAsync($"/pets/{petId}/activities");

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
        var response = await client.PostAsJsonAsync($"/pets/{petId}/activities", new { });

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
        var response = await client.PostAsJsonAsync($"/pets/{petId}/activities",
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

    private async Task<Pet> InsertPet(HttpClient client, string name, params Need[] needs)
    {
        var newPet = new Pet
        {
            Name = name,
            Breed = new Breed("German Shepherd"),
            Birthday = new DateTime(2020, 4, 1),
            Needs = needs.ToList()
        };
        var createdPet = await client.PostAsJsonAsync("/pets", newPet);
        await Task.Delay(5000); // bleh, but the create returns the created resource for the caller and we read from a different store
        return await createdPet.Content.ReadFromJsonAsync<Pet>();
    }
}