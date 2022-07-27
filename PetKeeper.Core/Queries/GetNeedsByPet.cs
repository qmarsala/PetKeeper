using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetNeedsByPet : IRequest<Option<List<Need>>>
{
    public string PetId { get; init; }
}

public class GetNeedsByPetHandler : IRequestHandler<GetNeedsByPet, Option<List<Need>>>
{
    public GetNeedsByPetHandler(IPetRepository petRepository)
    {
        PetRepository = petRepository;
    }

    public IPetRepository PetRepository { get; }

    public Task<Option<List<Need>>> Handle(GetNeedsByPet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(PetRepository.GetPetNeeds(request.PetId));
    }
}
