using MediatR;
using PetKeeper.Api.Responses;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Queries;

namespace PetKeeper.Api.Endpoints;

public static class ActivityLogEndpoints
{
    public static async Task<IResult> GetAllActivities(IMediator mediator, GetAllActivities query) =>
        (await mediator.Send(query))
            .Match(
                Some: al => Results.Ok(new ActivitiesResponse { Activities = al.Activities }),
                None: Results.NotFound("No activity log found."));

    public static async Task<IResult> GetActivitiesByPetId(IMediator mediator, GetActivitiesByPet query) =>
        (await mediator.Send(query))
            .Match(
                Some: al => Results.Ok(new ActivitiesResponse { Activities = al.Activities }),
                None: Results.NotFound());

    public static async Task<IResult> LogActivityForPet(IMediator mediator, AddActivityLog request) =>
        (await mediator.Send(request))
            .Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => e is PetNotFoundException ? Results.NotFound() : Results.StatusCode(500));
}