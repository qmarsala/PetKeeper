using LanguageExt;

namespace PetKeeper.Core.Interfaces;

public interface IReadPets
{
    Task<List<Pet>> GetAllPets();
    Task<Option<Pet>> GetPet(string petId);
}
