using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetAllPets : IRequest<Option<List<Pet>>>
{
}

public class GetAllPetsHandler : IRequestHandler<GetAllPets, Option<List<Pet>>>
{
    public GetAllPetsHandler(IPetRepository petRepository)
    {
        PetRepository = petRepository;
    }

    public IPetRepository PetRepository { get; }

    public Task<Option<List<Pet>>> Handle(GetAllPets request, CancellationToken cancellationToken)
    {
        return Task.FromResult(PetRepository.GetAllPets());
    }
}