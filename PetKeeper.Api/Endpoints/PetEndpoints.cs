using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Api.Responses;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Api.Endpoints;

public static class PetEndpoints
{
    public static IResult AddPet(IPetRepository petRepo, Pet newPet) =>
        petRepo.AddPet(newPet)
        .Match(
            Succ: p => Results.Created("pets", p),
            Fail: e => Results.StatusCode(500));

    public static IResult GetPets(IPetRepository petRepo) =>
        petRepo
        .GetAllPets()
        .Match(
            Some: ps => Results.Ok(new PetsResponse { Pets = ps.ToList() }),
            None: Results.NotFound("No pets found."));

    public static IResult GetPetById(IPetRepository petRepo, string petId) =>
        petRepo
        .GetPet(petId)
        .Match(
            Some: p => Results.Ok(p),
            None: Results.NotFound("No pet found."));

    public static IResult GetPetNeeds(IPetRepository petRepo, string petId) =>
        petRepo
        .GetPetNeeds(petId)
        .Match(
            Some: ns => Results.Ok(new PetNeedsResponse { PetNeeds = ns.ToList() }),
            None: Results.NotFound("No pet found."));

    public static IResult AddPetNeed(IPetRepository petRepo, IPetService petService, string petId, Need newNeed) =>
        petRepo
        .GetPet(petId)
        .Match(
            Some: p => petService
                .AddNeedToPet(p, newNeed)
                .Match(
                    Succ: n => Results.Created($"pets/{petId}/needs", n),
                    Fail: e => Results.StatusCode(500)),
            None: Results.NotFound("No pet found."));

    //this now loses the ability to 404 on no pet found
    // and we only get one failure... is this where maybe the either monad could help?
    // or is there some other way the resulting failure could 404? custom exception? :/
    public static IResult AddPetNeed2(IPetService petService, string petId, Need newNeed) =>
        petService
        .AddNeedToPet(petId, newNeed)
        .Match(
            Succ: n => Results.Created($"pets/{petId}/needs", n),
            Fail: e => Results.StatusCode(500));
}
