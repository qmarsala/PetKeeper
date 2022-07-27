using Microsoft.AspNetCore.Mvc;
using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using static PetKeeper.Api.Endpoints.PetEndpoints;
using static PetKeeper.Api.Endpoints.ActivityLogEndpoints;
using MediatR;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Queries;

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

app.MapGet("pets", async ([FromServices] IMediator mediator)
    => await GetPets(mediator, new()));

app.MapPost("pets", async ([FromServices] IMediator mediator, [FromBody] CreateNewPet request)
    => await CreateNewPet(mediator, request));

app.MapGet("pets/{petId}", async ([FromServices] IMediator mediator, string petId)
    => await GetPetById(mediator, new GetPet { PetId = petId }));

app.MapGet("pets/{petId}/needs", async ([FromServices] IMediator mediator, string petId)
    => await GetPetNeeds(mediator, new GetNeedsByPet{ PetId = petId }));

app.MapPost("pets/{petId}/needs",
    async ([FromServices] IMediator mediator, string petId, [FromBody] CreateNewNeedForPet request)
            => await CreateNewNeedForPet(mediator, request with { PetId = petId }));

app.MapPost("pets/{petId}/activities",
    async ([FromServices] IMediator mediator, string petId, [FromBody] AddActivityLog request)
    => await LogActivityForPet(mediator, request with { PetId = petId }));

app.MapGet("pets/activities", async ([FromServices] IMediator mediator)
    => await GetAllActivities(mediator, new()));

app.MapGet("pets/{petId}/activities", 
    async (IMediator mediator,string petId)
        => await GetActivitiesByPetId(mediator, new GetActivitiesByPet { PetId = petId }));

app.Run();
