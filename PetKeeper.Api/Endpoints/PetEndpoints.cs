using LanguageExt;
using LanguageExt.Common;
using MediatR;
using PetKeeper.Api.Responses;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Api.Endpoints;

public static class PetEndpoints
{
    public static async Task<IResult> CreateNewPet(IMediator mediator, CreateNewPet request) =>
        (await mediator.Send(request))
            .Match(
                Succ: p => Results.Created("pets", p),
                Fail: e => Results.StatusCode(500));

    public static async Task<IResult> CreateNewNeedForPet(IMediator mediator, CreateNewNeedForPet request) =>
        (await mediator.Send(request))
            .Match(
                Succ: p => Results.Created($"pets/{p.Id}/needs", p),
                Fail: e => e is PetNotFoundException 
                    ? Results.NotFound() 
                    : Results.StatusCode(500));

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
}
