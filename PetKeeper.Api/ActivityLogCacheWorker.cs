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
        //todo:
        // if we have not awaited yet, consume will block the foreground api
        // need to figure out a better way for this
        await Task.Delay(100, stoppingToken);

        Consumer.Subscribe(KafkaTopics.ActivityLog);
        var db = Redis.GetDatabase();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = Consumer.Consume(stoppingToken);
                if (result.IsPartitionEOF) continue;

                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                var key = result.Message.Key;
                var activityJson = result.Message.Value;
                if (activityJson is null)
                {
                    await RemoveActivity(db, key);
                }
                else
                {
                    await UpdateActivity(db, key, activityJson, result.Offset);
                }

                Consumer.StoreOffset(result);
                stopWatch.Stop();
                Console.WriteLine($"processed message in: {stopWatch.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task RemoveActivity(IDatabase db, string key)
    {
        var cachedActivityJson = await db.StringGetAsync(key);
        if (cachedActivityJson.HasValue)
        {
            var cachedActivity = JsonSerializer.Deserialize<CachedActivity>(cachedActivityJson!);
            var allPositionTask = db.ListPositionAsync("activities", cachedActivityJson);
            var petActivityListKey = $"{cachedActivity.Activity.PetId}.activities";
            var petPotistionTask = db.ListPositionAsync(petActivityListKey, cachedActivityJson);

            var results = await Task.WhenAll(allPositionTask, petPotistionTask);

            var removeActivities = Task.CompletedTask;
            var removePetActivities = Task.CompletedTask;
            if (results[0] > -1)
            {
                removeActivities = db.ListRemoveAsync("activities", cachedActivityJson);
            }
            if (results[1] > -1)
            {
                removePetActivities = db.ListRemoveAsync(petActivityListKey, cachedActivityJson);
            }
            var removeKeyTask = db.KeyDeleteAsync(key);
            await Task.WhenAll(removeKeyTask, removePetActivities, removeActivities);
        }
    }

    private async Task UpdateActivity(IDatabase db, string key, string activityJson, long offset)
    {
        var cachedActivityJson = await db.StringGetAsync(key);
        var cachedActivity = cachedActivityJson.HasValue
            ? JsonSerializer.Deserialize<CachedActivity>(cachedActivityJson!)
            : new CachedActivity();

        if (cachedActivity?.Offset < offset)
        {
            var updatedActivity = JsonSerializer.Deserialize<Activity>(activityJson);
            var updatedActivityJson = JsonSerializer.Serialize(new CachedActivity { Activity = updatedActivity!, Offset = offset });
            await RemoveActivity(db, key);
            var set = db.StringSetAsync(key, updatedActivityJson);
            var add = db.ListLeftPushAsync("activities", updatedActivityJson);
            var petActivityListKey = $"{cachedActivity.Activity.PetId}.activities";
            var add2 = db.ListLeftPushAsync(petActivityListKey, updatedActivityJson);
            await Task.WhenAll(set, add, add2);
        }
    }
}
