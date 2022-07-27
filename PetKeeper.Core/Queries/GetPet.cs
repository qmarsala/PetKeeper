using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetPet : IRequest<Option<Pet>>
{
    public string PetId { get; init; } = string.Empty;
}

public class GetPetHandler : IRequestHandler<GetPet, Option<Pet>>
{
    public GetPetHandler(IPetRepository petRepository)
    {
        PetRepository = petRepository;
    }

    public IPetRepository PetRepository { get; }

    public Task<Option<Pet>> Handle(GetPet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(PetRepository.GetPet(request.PetId));
    }
}
