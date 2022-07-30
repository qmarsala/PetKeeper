using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetKeeper.Api.Responses;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Queries;

namespace PetKeeper.Api.Endpoints;

public static class ActivityLogEndpoints
{
    public static IEndpointRouteBuilder RegisterAcitivyLogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("pets/{petId}/activities",
            async ([FromServices] IMediator mediator, string petId, [FromBody] AddActivityLog request)
                => await LogActivityForPet(mediator, request with { PetId = petId }));

        app.MapGet("pets/activities", async ([FromServices] IMediator mediator)
            => await GetAllActivities(mediator, new()));

        app.MapGet("pets/{petId}/activities",
            async (IMediator mediator, string petId)
                => await GetActivitiesByPetId(mediator, new GetActivitiesByPet { PetId = petId }));

        return app;
    }

    public static async Task<IResult> GetAllActivities(IMediator mediator, GetAllActivities query) =>
        (await mediator.Send(query))
            .Match(
                Some: al => Results.Ok(new ActivitiesResponse { Activities = al.Activities }),
                None: Results.NotFound("No activity log found."));

    public static async Task<IResult> GetActivitiesByPetId(IMediator mediator, GetActivitiesByPet query) =>
        (await mediator.Send(query))
            .Match(
                Some: al => Results.Ok(new ActivitiesResponse { Activities = al.Activities }),
                None: () => Results.NotFound());

    public static async Task<IResult> LogActivityForPet(IMediator mediator, AddActivityLog request) =>
        (await mediator.Send(request))
            .Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => e is PetNotFoundException ? Results.NotFound() : Results.StatusCode(500));
}