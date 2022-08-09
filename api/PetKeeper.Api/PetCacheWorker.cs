using Confluent.Kafka;
using static LanguageExt.Prelude;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using LanguageExt;
using static PetKeeper.Api.TestGround;
using Err = LanguageExt.Common.Error;
using static PetKeeper.Infrastructure.KafkaEff;
using static PetKeeper.Infrastructure.RedisEff;

namespace PetKeeper.Api;

//some of this went backwards and thats ok
// playing around with some function stuff (and failing a little)
// todo: keep going down this road

public static class TestGround
{

    public static Eff<ConsumeResult<string, string>> LogValue(ConsumeResult<string, string> value)
       => Eff(() => { Console.WriteLine($"{value.Message.Key}:{value.Message.Value}"); return value; });
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

        // I think we are missing something here...
        // not sure how the runtime is really supposed to work and I think we are missing the value with this
        // implementation
        // also not sure why we need to pass it to run unit - makes me think the closure is bad 
        // and there should be some rt variable for us... ???
        Consumer.Subscribe(KafkaTopics.Pets);
        var runtime = new LiveRuntime(Consumer, Redis);
        var operation =
            repeat(
                from value in runtime.KafkaEff.Map(c => c.Consume(stoppingToken))
                from _1 in LogValue(value)
                from _2 in value.Message.Value is null
                    ? runtime.RedisEff.Map(r => r.RemovePet(value))
                    : runtime.RedisEff.Map(r => r.UpdatePet(value))
                from _3 in runtime.KafkaEff.Map(c => c.StoreOffset(value))
                select _3);
        operation.RunUnit(runtime);

        // using the "non environment" eff 
        var db = Redis.GetDatabase();
        var useOperation1 = false;
        var operation1 =
            repeat(
                from _g in guard(useOperation1, Err.New("operation disabled"))
                from value in consumeTopic(Consumer)
                from _1 in LogValue(value)
                from _2 in value.Message.Value is null
                    ? removePet(db, value)
                    : updatePet(db, value)
                from _3 in storeOffset(Consumer, value)
                select _3);
        operation1.RunUnit();
    }
}
