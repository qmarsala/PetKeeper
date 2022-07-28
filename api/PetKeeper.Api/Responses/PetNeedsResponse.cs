using PetKeeper.Core;

namespace PetKeeper.Api.Responses;

public record PetNeedsResponse
{
    public List<Need> PetNeeds { get; init; } = new();
}