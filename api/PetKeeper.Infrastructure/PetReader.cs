using LanguageExt;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace PetKeeper.Infrastructure;

public class PetReader : IReadPets
{
    public PetReader(IConnectionMultiplexer redis)
    {
        Redis = redis;
    }

    public IConnectionMultiplexer Redis { get; }

    public async Task<List<Pet>> GetAllPets()
    {
        try
        {
            var db = Redis.GetDatabase();
            var result = await db.ListRangeAsync("pets");
            var pets = result
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(pJson => JsonSerializer.Deserialize<CachedPet>(pJson!)?.Pet)
                .Where(p => p is not null)
                .ToList();
            return pets!;
        }
        catch
        {
            return new();
        }
    }

    public async Task<Option<Pet>> GetPet(string petId)
    {
        try
        {
            var db = Redis.GetDatabase();
            var result = await db.StringGetAsync(petId);
            return result.HasValue
                ? JsonSerializer.Deserialize<CachedPet>(result!)?.Pet ?? Option<Pet>.None
                : Option<Pet>.None;
        }
        catch
        {
            return Option<Pet>.None;
        }
    }
}
