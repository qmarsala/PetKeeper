using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetKeeper.Api.Responses;
using PetKeeper.Core.Commands;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Queries;

namespace PetKeeper.Api.Endpoints;

public static class PetEndpoints
{
    public static IEndpointRouteBuilder RegisterPetEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("pets", async ([FromServices] IMediator mediator)
            => await GetPets(mediator, new()));

        app.MapPost("pets", async ([FromServices] IMediator mediator, [FromBody] CreateNewPet request)
            => await CreateNewPet(mediator, request));

        app.MapGet("pets/{petId}", async ([FromServices] IMediator mediator, string petId)
            => await GetPetById(mediator, new GetPet { PetId = petId }));

        app.MapGet("pets/{petId}/needs", async ([FromServices] IMediator mediator, string petId)
            => await GetPetNeeds(mediator, new GetNeedsByPet { PetId = petId }));

        app.MapPost("pets/{petId}/needs",
            async ([FromServices] IMediator mediator, string petId, [FromBody] CreateNewNeedForPet request)
                => await CreateNewNeedForPet(mediator, request with { PetId = petId }));
        return app;
    }

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

    public static async Task<IResult> GetPets(IMediator mediator, GetAllPets query) =>
        (await mediator.Send(query))
            .Match(
                Some: ps => Results.Ok(new PetsResponse { Pets = ps.ToList() }),
                None: Results.NotFound("No pets found."));

    public static async Task<IResult> GetPetById(IMediator mediator, GetPet query) =>
        (await mediator.Send(query))
            .Match(
                Some: p => Results.Ok(p),
                None: Results.NotFound("No pet found."));

    public static async Task<IResult> GetPetNeeds(IMediator mediator, GetNeedsByPet query) =>
        (await mediator.Send(query))
            .Match(
                Some: ns => Results.Ok(new PetNeedsResponse { PetNeeds = ns.ToList() }),
                None: Results.NotFound("No pet found."));
}
