using PetKeeper.Core;

namespace PetKeeper.Api.Responses;

public record ActivitiesResponse
{
    public List<Activity> Activities { get; init; } = new();
}
