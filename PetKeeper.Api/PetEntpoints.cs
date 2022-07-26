using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

public static class PetEntpoints
{
    public static Result<Pet> AddPet(IPetRepository petRepo, Pet newPet) => 
        petRepo.AddPet(newPet);

    public static Option<List<Pet>> GetPets(IPetRepository petRepo) =>
        petRepo.GetAllPets()
            .Map(ps => ps.ToList());

    public static Option<Pet> GetPet(IPetRepository petRepo, string petId) =>
        petRepo.GetPet(petId);


    public static Option<IEnumerable<Need>> GetNeeds(IPetRepository petRepo, string petId) =>
        petRepo.GetPetNeeds(petId);
}
