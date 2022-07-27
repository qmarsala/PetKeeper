using LanguageExt;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace PetKeeper.Infrastructure;

public class ActivityLogReader : IReadActivityLogs
{
    public ActivityLogReader(IConnectionMultiplexer redis)
    {
        Redis = redis;
    }

    public IConnectionMultiplexer Redis { get; }

    public async Task<ActivityLog> GetAllActivities()
    {
        try
        {
            var db = Redis.GetDatabase();
            var result = await db.ListRangeAsync("activities");
            return ReadActivityLog(result)
                .Match(
                    Some: al => al, 
                    None: new ActivityLog());
        }
        catch
        {
            return new();
        }
    }

    public async Task<Option<ActivityLog>> GetAllActivitiesForPet(string petId)
    {
        try
        {
            var db = Redis.GetDatabase();
            var result = await db.ListRangeAsync($"{petId}.activities");
            return ReadActivityLog(result);
        }
        catch
        {
            return Option<ActivityLog>.None;
        }
    }

    private static Option<ActivityLog> ReadActivityLog(RedisValue[] result)
    {
        var activities = result
           .Where(x => !string.IsNullOrEmpty(x))
           .Select(alJson => JsonSerializer.Deserialize<CachedActivity>(alJson!)?.Activity)
           .Where(al => al is not null)
           .ToList();
        return new ActivityLog { Activities = activities! };
    }
}

