using Microsoft.AspNetCore.Mvc;
using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using PetKeeper.Core;
using static PetKeeper.Api.Endpoints.PetEndpoints;
using static PetKeeper.Api.Endpoints.ActivityLogEndpoints;
using PetKeeper.Api.Requests;
using MediatR;
using PetKeeper.Core.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(typeof(CreateNewPetHandler));
builder.Services.AddScoped<IPetRepository, InMemoryPetsRepository>();
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

app.MapPost("pets", async ([FromServices] IMediator mediator, [FromBody] CreateNewPet request)
    => await CreateNewPet(mediator, request));

app.MapGet("pets/{petId}", ([FromServices] IPetRepository repo, string petId)
    => GetPetById(repo, petId));

app.MapGet("pets/{petId}/needs", ([FromServices] IPetRepository repo, string petId)
    => GetPetNeeds(repo, petId));

app.MapPost("pets/{petId}/needs",
    async ([FromServices] IMediator mediator, string petId, [FromBody] CreateNewNeedForPet request)
            => await CreateNewNeedForPet(mediator, request with { PetId = petId }));

app.MapPost("pets/{petId}/activities",
    async ([FromServices] IMediator mediator, string petId, [FromBody] AddActivityLog request)
    => await LogActivityForPet(mediator, request with { PetId = petId }));

app.MapGet("pets/activities", ([FromServices] IActivityLogRepository activityLogRepo)
    => GetAllActivities(activityLogRepo));

app.MapGet("pets/{petId}/activities", (
    [FromServices] IPetRepository petRepo,
    [FromServices] IActivityLogRepository activityLogRepo,
    string petId)
    => GetActivitiesByPetId(petRepo, activityLogRepo, petId));

app.Run();
