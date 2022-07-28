using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

//todo:
// look into language-ext more
// there are io monads and ways of dealing with async
public interface IWritePets
{
    Task<Result<Pet>> WritePet(Pet pet);
    Task<Result<Unit>> RemovePet(string petId);
}
