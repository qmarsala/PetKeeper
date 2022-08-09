using Confluent.Kafka;
using static LanguageExt.Prelude;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using LanguageExt;
using static PetKeeper.Infrastructure.Kafka<PetKeeper.Infrastructure.LiveRuntime>;
using static PetKeeper.Infrastructure.Redis<PetKeeper.Infrastructure.LiveRuntime>;

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
        Consumer.Subscribe(KafkaTopics.Pets);
        var runtime = new LiveRuntime(Consumer, Redis);
        var operation =
            repeat(
                from value in consumeTopic(stoppingToken)
                from _1 in value.Message.Value is null
                    ? removePet(value)
                    : updatePet(value)
                from _2 in storeOffset(value)
                select unit);
        operation.RunUnit(runtime);
    }
}
