namespace PetKeeper.Core;

public record Activity
{
    public string Id { get; init; }
    public string PetId { get; init; } = string.Empty;
    public string? NeedId { get; init; }
    public DateTime When { get; init; } = DateTime.Now;
    public string Notes { get; init; } = string.Empty;
}
