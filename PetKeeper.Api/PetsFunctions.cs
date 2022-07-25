using LanguageExt;

public static class PetsFunctions
{
    private static Pet Mooky = new()
    {
        Id = "abc123",
        Name = "Mooky",
        Birthday = new DateTime(2013, 6, 14),
        Breed = new Breed("German Shepherd"),
        Needs = new List<Need>
                {
                    new Need
                    {
                        Name = "Food",
                        Notes = "2 cups dry kibble - spoon of canned food mixed in",
                        Days = Enumerable.Range(0,7).Select(n => (DayOfWeek)n),
                        Times = 2
                    },
                    new Need
                    {
                        Name = "Pain Medication",
                        Notes = "Carboprofen - Give with Dinner", // how to "link" with dinner
                        Days = Enumerable.Range(0,7).Select(n => (DayOfWeek)n),
                        Times = 1
                    },
                    new Need
                    {
                        Name = "Training: Lesson 7",
                        Notes = "Let Mooky get to the end of the leash to practice recall",
                        Days = Enumerable.Range(0,7).Select(n => (DayOfWeek)n),
                        Times = 3
                    }
                }
    };

    public static Option<List<Pet>> GetPets() =>
        new List<Pet>
        {
            Mooky
        };

    public static Option<Pet> GetPet(string petId) =>
        petId == "abc123"
            ? Mooky
            : Option<Pet>.None;

    public static Option<IEnumerable<Need>> GetNeeds(string petId) =>
        GetPet(petId)
        .Match(
            Some: p => Option<IEnumerable<Need>>.Some(p.Needs),
            None: Option<IEnumerable<Need>>.None);
}
