using LanguageExt;
using PetKeeper.Api.Responses;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;

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
            None: Results.NotFound());

    public static IResult GetPetById(IPetRepository petRepo, string petId) =>
        petRepo
        .GetPet(petId)
        .Match(
            Some: p => Results.Ok(p),
            None: Results.NotFound());

    public static IResult GetPetNeeds(IPetRepository petRepo, string petId) =>
        petRepo
        .GetPetNeeds(petId)
        .Match(
            Some: ns => Results.Ok(new PetNeedsResponse { PetNeeds = ns.ToList() }),
            None: Results.NotFound());
}
