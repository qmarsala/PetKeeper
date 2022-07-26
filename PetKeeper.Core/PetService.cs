using LanguageExt.Common;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core;

public class PetService : IPetService
{
    private readonly IPetRepository PetRepository;

    public PetService(IPetRepository petRepository)
    {
        PetRepository = petRepository;
    }

    public Result<Need> AddNeedToPet(Pet pet, Need newNeed)
    {
        var need = newNeed with { Id = Guid.NewGuid().ToString() };
        var updatedPet = pet with { };
        updatedPet.Needs.Add(need);

        return PetRepository
            .UpdatePet(updatedPet)
            .Match(
                Succ: _ => need,
                Fail: e => new Result<Need>(e));
    }
}
