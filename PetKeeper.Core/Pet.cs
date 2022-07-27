namespace PetKeeper.Core;

public record Pet
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "A pet with no name";
    public DateTime Birthday { get; init; } = DateTime.Now;
    public Breed Breed { get; init; } = new Breed("Unknown");
    public List<Need> Needs { get; init; } = new();

    public Need AddNewNeed(Need need)
    {
        var newNeed = need with { Id = Guid.NewGuid().ToString() };
        Needs.Add(newNeed);
        return newNeed;
    }
}