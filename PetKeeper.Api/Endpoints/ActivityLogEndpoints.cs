using MediatR;
using PetKeeper.Api.Requests;
using PetKeeper.Api.Responses;
using PetKeeper.Core;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Interfaces;
using PetKeeper.Core.Queries;

namespace PetKeeper.Api.Endpoints;

public static class ActivityLogEndpoints
{
    public static async Task<IResult> GetAllActivities(IMediator mediator, GetAllActivities query) =>
        (await mediator.Send(query))
            .Match(
                Some: acs => Results.Ok(new ActivitiesResponse { Activities = acs }),
                None: Results.NotFound("No activity log found."));

    public static async Task<IResult> GetActivitiesByPetId(IMediator mediator, GetActivitiesByPet query) =>
        (await mediator.Send(query))
            .Match(
                Some: acs => Results.Ok(new ActivitiesResponse { Activities = acs }),
                None: Results.NotFound());

    public static async Task<IResult> LogActivityForPet(IMediator mediator, AddActivityLog request) =>
        (await mediator.Send(request))
            .Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => e is PetNotFoundException ? Results.NotFound() : Results.StatusCode(500));
}