using Confluent.Kafka;
using PetKeeper.Core;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;

namespace PetKeeper.Api;

public record CachedPet(Pet Pet, long Offset);

public class PetCacheWorker : BackgroundService
{
    public PetCacheWorker(IConsumer<string, string> consumer, IConnectionMultiplexer redis)
    {
        Consumer = consumer;
        Redis = redis;
    }

    public IConsumer<string, string> Consumer { get; }
    public IConnectionMultiplexer Redis { get; }

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
            var result = Consumer.Consume(stoppingToken);
            if (result.IsPartitionEOF) continue;

            var key = result.Message.Key;
            var petJson = result.Message.Value;
            if (petJson is null)
            {
                await db.KeyDeleteAsync(key);
            }
            else
            {
                var cachedPetJson = await db.StringGetAsync(key);
                var cachedPet = cachedPetJson.HasValue
                    ? JsonSerializer.Deserialize<CachedPet>(cachedPetJson!)
                    : new CachedPet(new(), -1);

                if (cachedPet?.Offset < result.Offset)
                {
                    var pet = JsonSerializer.Deserialize<Pet>(petJson);
                    var json = JsonSerializer.Serialize(new CachedPet(pet!, result.Offset));
                    await db.StringSetAsync(result.Message.Key, json);
                }
            }

            Consumer.StoreOffset(result);
        }
    }
}
