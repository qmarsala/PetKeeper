using LanguageExt;
using LanguageExt.Common;

public static class ActivityLogFunctions
{
    //temp db
    public static List<Activity> Activities = new List<Activity>();

    public static Option<List<Activity>> GetActivities() => Activities;

    public static Option<List<Activity>> GetActivities(string petId) => Activities.Where(a => a.PetId == petId).ToList();

    public static Result<Activity> LogActivity(Pet pet, Need need, string notes)
    {   
        var newActivity = new Activity
        {
            PetId = pet.Id,
            NeedId = need.Id,
            When = DateTime.Now,
            Notes = notes
        };
        Activities.Add(newActivity);
        return new Result<Activity>(newActivity);
    }   
}