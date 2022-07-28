using PetKeeper.Core;

namespace PetKeeper.Api.Responses;

public record PetsResponse
{
    public List<Pet> Pets { get; init; } = new();
}
