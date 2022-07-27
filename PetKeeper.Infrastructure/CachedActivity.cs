using PetKeeper.Core;

namespace PetKeeper.Infrastructure;

public record CachedActivity
{
    public Activity Activity { get; init; } = new();
    public long Offset { get; init; }
}

