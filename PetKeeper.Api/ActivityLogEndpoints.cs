using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

public static class ActivityLogEndpoints
{
    public static Option<List<Activity>> GetActivities(IActivityLogRepository repo) => 
        repo.GetAllActivities();

    public static Option<List<Activity>> GetActivities(IActivityLogRepository repo, string petId) => 
        repo.GetAllActivitiesForPet(petId);

    public static Result<Activity> LogActivity(IActivityLogRepository repo, string petId, string? needId, string notes) =>
        repo.AddActivityLog(new Activity
        {
            PetId = petId,
            NeedId = needId,
            When = DateTime.Now,
            Notes = notes
        });
}