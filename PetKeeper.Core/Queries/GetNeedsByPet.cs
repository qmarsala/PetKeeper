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
    public GetNeedsByPetHandler(IReadPets petReader)
    {
        PetReader = petReader;
    }

    public IReadPets PetReader { get; }

    public async Task<Option<List<Need>>> Handle(GetNeedsByPet request, CancellationToken cancellationToken)
        => (await PetReader.GetPet(request.PetId))
            .Map(p => p.Needs);
}
