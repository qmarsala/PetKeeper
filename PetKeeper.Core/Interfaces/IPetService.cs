using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

public interface IPetService
{
    Result<Need> AddNeedToPet(Pet p, Need newNeed);
}
