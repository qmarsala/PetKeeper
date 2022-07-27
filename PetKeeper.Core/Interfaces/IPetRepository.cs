using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

//todo:
// look into language-ext more
// there are io monads and ways of dealing with async
public interface IWritePets
{
    Task<Result<Pet>> WritePet(Pet pet);
}

public interface IReadPets
{
    Task<List<Pet>> GetAllPets();
    Task<Option<Pet>> GetPet(string petId);
    Task<Option<List<Need>>> GetPetNeeds(string petId);
}

public interface IPetRepository
{
    Option<List<Pet>> GetAllPets();
    Option<Pet> GetPet(string petId);
    Result<Pet> AddPet(Pet newPet);
    Option<List<Need>> GetPetNeeds(string petId);
    Result<Pet> UpdatePet(Pet updatedPet);
}