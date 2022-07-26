using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

public interface IPetRepository
{
    Option<IEnumerable<Pet>> GetAllPets();
    Option<Pet> GetPet(string petId);
    Result<Pet> AddPet(Pet newPet);
    Option<IEnumerable<Need>> GetPetNeeds(string petId);
}