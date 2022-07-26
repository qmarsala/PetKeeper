namespace PetKeeper.Core;

public record Pet
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "A pet with no name";
    public DateTime Birthday { get; init; } = DateTime.Now;
    public Breed Breed { get; init; } = new Breed("Unknown");
    public List<Need> Needs { get; init; } = new();
}