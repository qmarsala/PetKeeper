using Confluent.Kafka;
using LanguageExt;
using static LanguageExt.Prelude;
using StackExchange.Redis;
using System.Text.Json;
using PetKeeper.Core;

namespace PetKeeper.Infrastructure;

public interface RedisIO
{
    Unit RemovePet(ConsumeResult<string, string> value);
    Unit UpdatePet(ConsumeResult<string, string> value);
}

public interface HasRedis<RT>
    where RT : struct, HasRedis<RT>
{
    Eff<RT, RedisIO> RedisEff { get; }
}

//maybe?
// how are we supposed to capture this IO?
public struct LiveRedisIO : RedisIO
{
    private readonly IDatabase Db;

    public LiveRedisIO(IConnectionMultiplexer connectionMultiplexer)
    {
        Db = connectionMultiplexer.GetDatabase();
    }

    public LiveRedisIO(IDatabase db)
    {
        Db = db;
    }

    public static RedisIO Create(IConnectionMultiplexer connectionMultiplexer) => new LiveRedisIO(connectionMultiplexer);
    public static RedisIO Create(IDatabase db) => new LiveRedisIO(db);

    public Unit RemovePet(ConsumeResult<string, string> value)
    {
        var key = value.Message.Key;
        var petToRemove = Db.StringGetAsync(key).Result;
        if (petToRemove.HasValue)
        {
            var positions = Db.ListPositionsAsync("pets", petToRemove, 1).Result;
            if (positions.Any())
            {
                _ = Db.ListRemoveAsync("pets", petToRemove).Result;
            }
            _ = Db.KeyDeleteAsync(key).Result;
        }
        return unit;
    }

    public Unit UpdatePet(ConsumeResult<string, string> value)
    {
        var key = value.Message.Key;
        var petJson = value.Message.Value;
        var offset = value.Offset;
        var cachedPetJson = Db.StringGetAsync(key as string).Result;
        var cachedPet = cachedPetJson.HasValue
            ? JsonSerializer.Deserialize<CachedPet>(cachedPetJson!)
            : new CachedPet();

        // this isn't really good enough, 
        // the offset could be lesser for a newer event 
        // if we have increased partitions or versioned our topic
        // todo: need something better
        if (cachedPet?.Offset < offset)
        {
            var updatedPet = JsonSerializer.Deserialize<Pet>(petJson);
            var updatedPetJson = JsonSerializer.Serialize(new CachedPet { Pet = updatedPet!, Offset = offset });
            var petToRemove = Db.StringGetAsync(key).Result;
            RemovePet(value);
            _ = Db.StringSetAsync(key, updatedPetJson).Result;
            _ = Db.ListLeftPushAsync("pets", updatedPetJson).Result;
        }
        return unit;
    }
}

// this seemed simpler - but the runtime is supposed to be cool?
public static class RedisEff
{
    public static Eff<Unit> removePet(IDatabase db, ConsumeResult<string, string> value) =>
        Eff(() => LiveRedisIO.Create(db).RemovePet(value));

    public static Eff<Unit> updatePet(IDatabase db, ConsumeResult<string, string> value) =>
        Eff(() => LiveRedisIO.Create(db).UpdatePet(value));
}