using PetKeeper.Core;

namespace PetKeeper.Infrastructure;

public record CachedPet(Pet Pet, long Offset);
