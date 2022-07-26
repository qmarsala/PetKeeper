using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

public static class ActivityLogEndpoints
{
    public static IResult GetAllActivities(IActivityLogRepository repo) =>
        repo
        .GetAllActivities()
        .Match(
            Some: acs => Results.Ok(new { Activities = acs }),
            None: Results.NotFound());

    public static IResult GetActivitiesByPetId(IPetRepository petRpeo, IActivityLogRepository activityLogRepo, string petId) =>
        petRpeo
        .GetPet(petId)
        .Map(p => activityLogRepo.GetAllActivitiesForPet(p.Id))
        .Match(
            Some: oa =>
                oa.Match(
                    Some: a => Results.Ok(new ActivitiesResponse { Activities = a }),
                    None: Results.NotFound()),
            None: Results.NotFound());

    public static IResult LogActivityForPet(
        IPetRepository petRepo, IActivityLogRepository activityLogRepo, string petId, LogActivityRequest request) =>
        petRepo
        .GetPet(petId)
        .Map(p => activityLogRepo.AddActivityLog(new Activity
        {
            PetId = p.Id,
            NeedId = request.NeedId,
            When = DateTime.Now,
            Notes = request.Notes
        }))
        .Match(
            Some: ra => ra.Match(
                Succ: a => Results.Created("activities", a),
                Fail: e => Results.StatusCode(500)),
            None: Results.NotFound());
}