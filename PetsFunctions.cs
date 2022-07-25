using LanguageExt;
using LanguageExt.Common;

public static class PetsFunctions
{
    public static Option<List<object>> GetPets() =>
        new List<object> { new { Id = "abc123", Name = "Mooky" } };

    public static Option<object> GetPet(string petId) =>
        petId == "abc123"
            ? new { Id = "abc123", Name = "Mooky" }
            : Option<object>.None;

    public static Option<List<object>> GetNeeds(string petId) =>
        GetPet(petId)
        .Match<Option<List<object>>>(
            Some: p => new List<object> {
                    new {
                        Id = "n1",
                        Type = "Food",
                        What = "Royal Canine",
                        // obviosly not a free form string 
                        // we want a concept of 'When' so we can filter on upcoming/now
                        // more like a schedule/cadence than an actual time.
                        When = "2x Daily - Breakfast, Dinner",
                        Notes = "2 cups dry kibble - spoon of canned food mixed in"
                    },
                    new {
                        Id = "n2",
                        Type = "Medication",
                        What = "Carboprofin",
                        // When needs to be some sort of date type 
                        // but associating to other events needs to be considered
                        When = "With Dinner",
                        Notes = "For pain management"
                    },
                    new {
                        Id = "n3",
                        Type = "Training",
                        What = "Lesson 7",
                        When = "3x per day",
                        Notes = "Let mooky get to the end of the leash and recall."
                    }
                },
            None: Option<List<object>>.None);


    //temp db
    public static List<Activity> Activities = new List<Activity>();

    //This would probably be a `Need` coming in
    public static Result<Activity> LogActivity(string petId, Activity activity) =>
        GetActivities(petId)
        .Match(
            Some: acs =>
            {
                var newActivity = activity with { When = DateTime.Now };
                acs.Add(newActivity);
                return new Result<Activity>(newActivity);
            },
            None: new Result<Activity>(new Exception("Error adding activity")));

    public static Option<List<Activity>> GetActivities(string petId) =>
        GetPet(petId)
        .Match(
            Some: p => Activities,
            None: Option<List<Activity>>.None);
}
