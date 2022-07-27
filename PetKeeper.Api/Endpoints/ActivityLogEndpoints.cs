using MediatR;
using PetKeeper.Api.Requests;
using PetKeeper.Api.Responses;
using PetKeeper.Core;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Api.Endpoints;

public static class ActivityLogEndpoints
{
    public static IResult GetAllActivities(IActivityLogRepository repo) =>
        repo
        .GetAllActivities()
        .Match(
            Some: acs => Results.Ok(new ActivitiesResponse { Activities = acs }),
            None: Results.NotFound("No activity log found."));

    public static IResult GetActivitiesByPetId(IPetRepository petRpeo, IActivityLogRepository activityLogRepo, string petId) =>
        petRpeo
        .GetPet(petId)
        .Map(p => activityLogRepo.GetAllActivitiesForPet(p.Id))
        .Match(
            Some: oa =>
                oa.Match(
                    Some: acs => Results.Ok(new ActivitiesResponse { Activities = acs }),
                    None: Results.NotFound("No activity log found.")),
            None: Results.NotFound("No pet found."));

    public static async Task<IResult> LogActivityForPet(IMediator mediator, AddActivityLog request) =>
        (await mediator.Send(request))
            .Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => e is PetNotFoundException ? Results.NotFound() : Results.StatusCode(500));
}