using LanguageExt;
using LanguageExt.Common;

public static class ActivityLogEntpoints
{
    //temp db
    public static List<Activity> Activities = new List<Activity>();

    public static Option<List<Activity>> GetActivities() => Activities;

    public static Option<List<Activity>> GetActivities(string petId) => Activities.Where(a => a.PetId == petId).ToList();

    public static Result<Activity> LogActivity(string petId, string? needId, string notes)
    {
        var newActivity = new Activity
        {
            PetId = petId,
            NeedId = needId,
            When = DateTime.Now,
            Notes = notes
        };
        Activities.Add(newActivity);
        return new Result<Activity>(newActivity);
    }
}