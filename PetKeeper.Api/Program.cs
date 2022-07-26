using Microsoft.AspNetCore.Mvc;
using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using PetKeeper.Core;
using static PetEndpoints;
using static ActivityLogEndpoints;
using PetKeeper.Api.Requests;

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

app.MapGet("pets", ([FromServices] IPetRepository repo) 
    => GetPets(repo));

app.MapPost("pets", ([FromServices] IPetRepository repo, [FromBody] Pet request) 
    => AddPet(repo, request));

app.MapGet("pets/{petId}", ([FromServices] IPetRepository repo, string petId) 
    => GetPetById(repo, petId));

app.MapGet("pets/{petId}/needs", ([FromServices] IPetRepository repo, string petId) 
    => GetPetNeeds(repo, petId));

app.MapPost("pets/{petId}/activities", (
    [FromServices] IPetRepository petRepo,
    [FromServices] IActivityLogRepository activityLogRepo,
    string petId,
    [FromBody] LogActivityRequest request) 
    => LogActivityForPet(petRepo, activityLogRepo, petId, request));

app.MapGet("pets/activities", ([FromServices] IActivityLogRepository activityLogRepo)
    => GetAllActivities(activityLogRepo));

app.MapGet("pets/{petId}/activities", (
    [FromServices] IPetRepository petRepo,
    [FromServices] IActivityLogRepository activityLogRepo,
    string petId)
    => GetActivitiesByPetId(petRepo, activityLogRepo, petId));

app.Run();
