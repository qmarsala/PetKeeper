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
    public CreateNewPetHandler(IWritePets petWriter)
    {
        PetWriter = petWriter;
    }

    public IWritePets PetWriter { get; }

    public async Task<Result<Pet>> Handle(CreateNewPet request, CancellationToken cancellationToken)
    {
        var newPet = new Pet
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Birthday = request.Birthday,
            Breed = request.Breed,
            Needs = Enumerable.Concat(
                request.Needs.Where(n => !string.IsNullOrWhiteSpace(n.Id)),
                request.Needs
                    .Where(n => string.IsNullOrWhiteSpace(n.Id))
                    .Select(n => n with { Id = Guid.NewGuid().ToString() }))
                .ToList()
        };
        return await PetWriter.WritePet(newPet);
    }
}
