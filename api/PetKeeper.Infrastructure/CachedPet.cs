using PetKeeper.Core;

namespace PetKeeper.Infrastructure;

public record CachedPet
{
    public Pet Pet { get; init; } = new();
    public long Offset { get; init; } = -1;
};
