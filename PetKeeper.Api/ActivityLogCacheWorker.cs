using Confluent.Kafka;
using PetKeeper.Core;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;

namespace PetKeeper.Api;

public class ActivityLogCacheWorker : BackgroundService
{
    public ActivityLogCacheWorker(IConsumer<string, string> consumer, IConnectionMultiplexer redis)
    {
        Consumer = consumer;
        Redis = redis;
    }

    public IConsumer<string, string> Consumer { get; }
    public IConnectionMultiplexer Redis { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(100, stoppingToken);

        Consumer.Subscribe(KafkaTopics.ActivityLog);
        var db = Redis.GetDatabase();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = Consumer.Consume(stoppingToken);
                if (result.IsPartitionEOF) continue;

                var key = result.Message.Key;
                var activityLogJson = result.Message.Value;
                if (activityLogJson is null)
                {
                    var activityToRmove = await db.StringGetAsync(key);
                    if (activityToRmove.HasValue)
                    {
                        await db.ListRemoveAsync("activities", activityToRmove);
                    }
                    await db.KeyDeleteAsync(key);
                }
                else
                {
                    var cachedActivityJson = await db.StringGetAsync(key);
                    var cachedActivity = cachedActivityJson.HasValue
                        ? JsonSerializer.Deserialize<CachedActivity>(cachedActivityJson!)
                        : new CachedActivity();

                    if (cachedActivity?.Offset < result.Offset)
                    {
                        var activity = JsonSerializer.Deserialize<Activity>(activityLogJson);
                        var json = JsonSerializer.Serialize(new CachedActivity { Activity = activity!, Offset = result.Offset });
                        if (cachedActivityJson.HasValue)
                        {
                            await db.ListRemoveAsync($"{activity.PetId}.activities", cachedActivityJson);
                        }

                        await db.StringSetAsync(key, json);
                        await db.ListLeftPushAsync("activities", json);
                        await db.ListLeftPushAsync($"{activity.PetId}.activities", json);
                    }
                }

                Consumer.StoreOffset(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
