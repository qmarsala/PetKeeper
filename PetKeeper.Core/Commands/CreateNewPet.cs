using LanguageExt;
using LanguageExt.Common;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Commands;

public record CreateNewPet : IRequest<Result<Pet>>
{
    public string Name { get; init; } = "A pet with no name";
    public DateTime Birthday { get; init; } = DateTime.Now;
    public Breed Breed { get; init; } = new Breed("Unknown");
    public List<Need> Needs { get; init; } = new();
}

public class CreateNewPetHandler : IRequestHandler<CreateNewPet, Result<Pet>>
{
    public CreateNewPetHandler(IPetRepository petRepository)
    {
        PetRepository = petRepository;
    }

    public IPetRepository PetRepository { get; }

    public Task<Result<Pet>> Handle(CreateNewPet request, CancellationToken cancellationToken)
    {
        var newPet = new Pet
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Birthday = request.Birthday,
            Breed = request.Breed,
            Needs = request.Needs
        };
        return Task.FromResult(PetRepository.AddPet(newPet));
    }
}
