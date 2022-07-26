using Microsoft.AspNetCore.Mvc;
using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using PetKeeper.Core;
using static PetEndpoints;
using static ActivityLogEndpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IPetRepository, PetsRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
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


app.MapPost("pets/{petId}/activities", (
    [FromServices] IPetRepository petRepo,
    [FromServices] IActivityLogRepository activityLogRepo,
    string petId, 
    [FromBody] LogActivityRequest request) =>
    GetPet(petRepo, petId)
    .Map(p => LogActivity(activityLogRepo, p.Id, request.NeedId, request.Notes))
    .Match(
        Some: ra =>
            ra.Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => Results.StatusCode(500)),
        None: Results.NotFound()));

app.MapGet("pets/activities", ([FromServices] IActivityLogRepository activityLogRepo) =>
    GetActivities(activityLogRepo)
    .Match(
        Some: acs => Results.Ok(new { Activities = acs }),
        None: Results.NotFound()));

app.MapGet("pets/{petId}/activities", (
    [FromServices] IPetRepository petRepo,
    [FromServices] IActivityLogRepository activityLogRepo,
    string petId) =>
    GetPet(petRepo, petId)
    .Map(p => GetActivities(activityLogRepo, p.Id))
    .Match(
        Some: acs => Results.Ok(new { Activities = acs }),
        None: Results.NotFound()));

app.Run();

public record LogActivityRequest(string? NeedId, string Notes);

public record PetsResponse
{
    public List<Pet> Pets { get; init; } = new();
}

public record ActivitiesResponse
{
    public List<Activity> Activities { get; init; } = new();
}