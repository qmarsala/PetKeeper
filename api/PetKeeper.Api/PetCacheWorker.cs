using Confluent.Kafka;
using static LanguageExt.Prelude;
using PetKeeper.Core;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using LanguageExt;
using static PetKeeper.Api.TestGround;
using Err = LanguageExt.Common.Error;

namespace PetKeeper.Api;

//some of this went backwards and thats ok
// playing around with some function stuff (and failing a little)
// todo: keep going down this road

// need to explore "environments" with the effects
// need to explore aff's (async effects)
// need to improve redis interaction

public static class TestGround
{
    // enumerator ???
    public static IEnumerable<ConsumeResult<TKey, TValue>> ReadPetTopic<TKey, TValue>(this IConsumer<TKey, TValue> consumer, CancellationToken stoppingToken)
    {
        // err handling with either?
        yield return consumer.Consume(stoppingToken);
    }

    // eff monads? - probably should be affs to be async
    //public static Eff<ConsumeResult<string, string>> ReadTopic(IConsumer<string, string> consumer, CancellationToken stoppingToken)
    //    => Eff(() => consumer.Consume(stoppingToken));
    public static Eff<ConsumeResult<string, string>> LogValue(ConsumeResult<string, string> value)
       => Eff(() => { Console.WriteLine($"{value.Message.Key}:{value.Message.Value}"); return value; });

    // how could we improve the redis part of this?
    // a redis aff? - can we use effs and affs together?
    public static Eff<ConsumeResult<string, string>> UpdatePet(ConsumeResult<string, string> value, IDatabase db)
        => Eff(() =>
        {
            var key = value.Message.Key;
            var petJson = value.Message.Value;
            var offset = value.Offset;
            var cachedPetJson = db.StringGetAsync(key as string).Result;
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
                var petToRemove = db.StringGetAsync(key).Result;
                // delete dup has made it back
                if (petToRemove.HasValue)
                {
                    var positions = db.ListPositionsAsync("pets", petToRemove, 1).Result;
                    if (positions.Any())
                    {
                        var r = db.ListRemoveAsync("pets", petToRemove).Result;
                    }
                    var d = db.KeyDeleteAsync(key).Result;
                }
                var s = db.StringSetAsync(key, updatedPetJson).Result;
                var lp = db.ListLeftPushAsync("pets", updatedPetJson).Result;
            }
            return value;
        });

    public static Eff<ConsumeResult<string, string>> RemovePet(ConsumeResult<string, string> value, IDatabase db)
        => Eff(() =>
        {
            var key = value.Message.Key;
            var petToRemove = db.StringGetAsync(key).Result;
            if (petToRemove.HasValue)
            {
                var positions = db.ListPositionsAsync("pets", petToRemove, 1).Result;
                if (positions.Any())
                {
                    var r = db.ListRemoveAsync("pets", petToRemove).Result;
                }
                var d = db.KeyDeleteAsync(key).Result;
            }
            return value;
        });
}

public class PetCacheWorker : BackgroundService
{
    public PetCacheWorker(IConsumer<string, string> consumer, IConnectionMultiplexer redis)
    {
        Consumer = consumer;
        Redis = redis;
    }

    public IConsumer<string, string> Consumer { get; }
    public IConnectionMultiplexer Redis { get; }

    //challenge: functional | inprogress
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //todo:
        // if we have not awaited yet, consume will block the foreground api
        // need to figure out a better way for this
        await Task.Delay(100, stoppingToken);

        Consumer.Subscribe(KafkaTopics.Pets);
        var db = Redis.GetDatabase();
        // I think we are missing something here...
        // not sure how the runtime is really supposed to work and I think we are missing the value with this
        // implementation
        // also not sure why we need to pass it to run unit - makes me think the closure is bad 
        // and there should be some rt variable for us... ???
        var liveKafka = new LiveRuntime(Consumer);
        var operation =
            repeat(
                from value in liveKafka.KafkaEff.Map(c => c.Consume(stoppingToken))
                from _1 in LogValue(value)
                from _2 in value.Message.Value is null
                    ? RemovePet(value, db)
                    : UpdatePet(value, db)
                from _3 in liveKafka.KafkaEff.Map(c => c.StoreOffset(value))
                select _3);
        operation.RunUnit(liveKafka);

        // using the "non environment" eff 
        var useOperation1 = false;
        var operation1 =
            repeat(
                from _g in guard(useOperation1, Err.New("operation disabled"))
                from value in KafkaEff.consumeTopic(Consumer)
                from _1 in LogValue(value)
                from _2 in value.Message.Value is null
                    ? RemovePet(value, db)
                    : UpdatePet(value, db)
                from _3 in KafkaEff.storeOffset(Consumer, value)
                select _3);
        operation1.RunUnit();
    }
}
