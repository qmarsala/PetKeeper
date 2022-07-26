using Microsoft.AspNetCore.Mvc;

using static PetsFunctions;
using static ActivityLogFunctions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.MapGet("pets", () =>
    GetPets()
    .Match(
        Some: ps => Results.Ok(new { Pets = ps }),
        None: Results.NotFound()));

app.MapGet("pets/{petId}", (string petId) =>
    GetPet(petId)
    .Match(
        Some: p => Results.Ok(p),
        None: Results.NotFound()));

app.MapGet("pets/{petId}/needs", (string petId) =>
    GetNeeds(petId)
    .Match(
        Some: ns => Results.Ok(new { Needs = ns }),
        None: Results.NotFound()));


app.MapPost("pets/{petId}/activities", (string petId, [FromBody] LogActivityRequest request) =>
    GetPet(petId)
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