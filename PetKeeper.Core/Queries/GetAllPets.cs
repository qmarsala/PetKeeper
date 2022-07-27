using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetAllPets : IRequest<Option<List<Pet>>>
{
}

public class GetAllPetsHandler : IRequestHandler<GetAllPets, Option<List<Pet>>>
{
    public GetAllPetsHandler(IReadPets petReader)
    {
        PetReader = petReader;
    }

    public IReadPets PetReader { get; }

    public async Task<Option<List<Pet>>> Handle(GetAllPets request, CancellationToken cancellationToken) 
        => await PetReader.GetAllPets();
}