using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

public interface IPetRepository
{
    Option<List<Pet>> GetAllPets();
    Option<Pet> GetPet(string petId);
    Result<Pet> AddPet(Pet newPet);
    Option<List<Need>> GetPetNeeds(string petId);
    Result<Pet> UpdatePet(Pet updatedPet);
}