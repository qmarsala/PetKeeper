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
    public GetPetHandler(IReadPets petReader)
    {
        PetReader = petReader;
    }

    public IReadPets PetReader { get; }

    public async Task<Option<Pet>> Handle(GetPet request, CancellationToken cancellationToken) 
        =>  await PetReader.GetPet(request.PetId);
}
