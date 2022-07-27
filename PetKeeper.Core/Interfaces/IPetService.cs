using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

public interface IPetService
{
    Result<Need> AddNeedToPet(Pet pet, Need newNeed);
    Result<Need> AddNeedToPet(string petId, Need newNeed);
}
