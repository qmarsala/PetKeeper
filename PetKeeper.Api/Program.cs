using Microsoft.AspNetCore.Mvc;

using static PetEntpoints;
using static ActivityLogEntpoints;
using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using PetKeeper.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IPetRepository, PetsRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("pets", ([FromServices] IPetRepository repo) =>
    GetPets(repo)
    .Match(
        Some: ps => Results.Ok(new PetsResponse { Pets = ps }),
        None: Results.NotFound()));

app.MapPost("pets", ([FromServices] IPetRepository repo, [FromBody] Pet request) =>
    AddPet(repo, request)
    .Match(
        Succ: p => Results.Created("pets", p),
        Fail: e => Results.StatusCode(500)));

app.MapGet("pets/{petId}", ([FromServices] IPetRepository repo, string petId) =>
    GetPet(repo, petId)
    .Match(
        Some: p => Results.Ok(p),
        None: Results.NotFound()));

app.MapGet("pets/{petId}/needs", ([FromServices] IPetRepository repo, string petId) =>
    GetNeeds(repo, petId)
    .Match(
        Some: ns => Results.Ok(new { Needs = ns }),
        None: Results.NotFound()));


app.MapPost("pets/{petId}/activities", ([FromServices] IPetRepository repo, string petId, [FromBody] LogActivityRequest request) =>
    GetPet(repo, petId)
    .Map(_ => LogActivity(petId, request.NeedId, request.Notes))
    .Match(
        Some: ra =>
            ra.Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => Results.StatusCode(500)),
        None: Results.NotFound()));

app.MapGet("pets/{petId}/activities", (string petId) =>
    GetActivities(petId)
    .Match(
        Some: acs => Results.Ok(new { Activities = acs }),
        None: Results.NotFound()));

app.MapGet("pets/activities", () =>
    GetActivities()
    .Match(
        Some: acs => Results.Ok(new { Activities = acs }),
        None: Results.NotFound()));

app.Run();

public record LogActivityRequest(string? NeedId, string Notes);

public record PetsResponse
{
    public List<Pet> Pets { get; init; } = new List<Pet>();
}