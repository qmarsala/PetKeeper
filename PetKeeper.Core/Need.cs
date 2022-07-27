namespace PetKeeper.Core;

public record Need
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = "An unknown need";
    public string Notes { get; init; } = string.Empty;
    public int Times { get; init; } = 1;
    public IEnumerable<DayOfWeek> Days { get; init; } = Enumerable.Empty<DayOfWeek>();
}
