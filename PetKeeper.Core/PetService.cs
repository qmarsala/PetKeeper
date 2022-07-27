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
        var updatedPet = pet with { };
        var need = updatedPet.AddNewNeed(newNeed);

        return PetRepository
            .UpdatePet(updatedPet)
            .Match(
                Succ: _ => need,
                Fail: e => new Result<Need>(e));
    }

    public Result<Need> AddNeedToPet(string petId, Need newNeed) =>
        PetRepository
            .GetPet(petId)
            .Match(
                Some: p =>
                {
                    var need = p.AddNewNeed(newNeed);
                    return PetRepository
                        .UpdatePet(p)
                        .Match(
                            Succ: _ => need,
                            Fail: e => new Result<Need>(e));
                },
                None: new Result<Need>(new Exception("No pet found")));
}
