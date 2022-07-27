using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Infrastructure;

public class InMemoryPetsRepository : IPetRepository
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
    private static readonly List<Pet> Pets = new();
    static InMemoryPetsRepository()
    {
        Pets.Add(Mooky);
    }

    public Result<Pet> AddPet(Pet newPet)
    {
        Pets.Add(newPet);
        return newPet;
    }

    public Result<Pet> UpdatePet(Pet updatedPet)
    {
        var petToUpdate = Pets.FirstOrDefault(p => p.Id == updatedPet.Id);
        if (petToUpdate is null)
        {
            return new Result<Pet>(new Exception("No pet to update."));
        }
        Pets.Remove(petToUpdate);
        Pets.Add(updatedPet);
        return updatedPet;
    }

    public Option<List<Pet>> GetAllPets() => Pets;

    public Option<Pet> GetPet(string petId) =>
        Pets.Any(p => p.Id == petId)
            ? Pets.First(p => p.Id == petId)
            : Option<Pet>.None;

    public Option<List<Need>> GetPetNeeds(string petId) =>
        GetPet(petId)
        .Map(p => p.Needs);
}
