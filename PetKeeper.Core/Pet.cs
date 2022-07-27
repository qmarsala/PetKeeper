namespace PetKeeper.Core;

public record Pet
{
    public string Id { get; init; }
    public string Name { get; init; } = "A pet with no name";
    public DateTime Birthday { get; init; } = DateTime.Now;
    public Breed Breed { get; init; } = new Breed("Unknown");
    public List<Need> Needs { get; init; } = new();

    public Need AddNeed(Need need)
    {
        Needs.Add(need);
        return need;
    }
}