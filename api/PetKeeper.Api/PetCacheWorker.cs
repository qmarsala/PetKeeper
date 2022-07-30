using Confluent.Kafka;
using PetKeeper.Core;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;

namespace PetKeeper.Api;

public class PetCacheWorker : BackgroundService
{
    public PetCacheWorker(IConsumer<string, string> consumer, IConnectionMultiplexer redis)
    {
        Consumer = consumer;
        Redis = redis;
    }

    public IConsumer<string, string> Consumer { get; }
    public IConnectionMultiplexer Redis { get; }

    //todo: challenge: functional?
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //todo:
        // if we have not awaited yet, consume will block the foreground api
        // need to figure out a better way for this
        await Task.Delay(100, stoppingToken);

        Consumer.Subscribe(KafkaTopics.Pets);
        var db = Redis.GetDatabase();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = Consumer.Consume(stoppingToken);
                if (result.IsPartitionEOF) continue;

                var key = result.Message.Key;
                var petJson = result.Message.Value;
                await (petJson is null 
                    ? RemovePet(db, key) 
                    : UpdatePet(db, key, petJson, result.Offset));

                Consumer.StoreOffset(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task RemovePet(IDatabase db, string key)
    {
        var petToRemove = await db.StringGetAsync(key);
        if (petToRemove.HasValue)
        {
            var positions = await db.ListPositionsAsync("pets", petToRemove, 1);
            if (positions.Any())
            {
                await db.ListRemoveAsync("pets", petToRemove);
            }
            await db.KeyDeleteAsync(key);
        }
    }

    private async Task UpdatePet(IDatabase db, string key, string petJson, long offset)
    {
        var cachedPetJson = await db.StringGetAsync(key);
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
            await RemovePet(db, key);
            await db.StringSetAsync(key, updatedPetJson);
            await db.ListLeftPushAsync("pets", updatedPetJson);
        }
    }
}
