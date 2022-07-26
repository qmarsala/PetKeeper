using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Infrastructure;

public class ActivityLogRepository : IActivityLogRepository
{
    public static List<Activity> Activities = new();

    public Result<Activity> AddActivityLog(Activity newActivity) 
    {
        var activity = newActivity with { Id = Guid.NewGuid().ToString() };
        Activities.Add(activity);
        return activity;
    }

    public Option<List<Activity>> GetAllActivities() => Activities;

    public Option<List<Activity>> GetAllActivitiesForPet(string petId) => 
        Activities.Where(a => a.PetId == petId).ToList();
}

