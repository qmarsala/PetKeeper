﻿using Confluent.Kafka;
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
        var testSource = new CancellationTokenSource();
        testSource.CancelAfter(10_000);

        // I think we are missing something here...
        // not sure how the runtime is really supposed to work and I think we are missing the value with this
        // implementation
        // perhaps we want to pull out our "operation" and then we could run it 
        // against a "testruntime" in our unit tests?
        // and then the worker provides the "liveruntime"?
        var processStream = repeat(
            from value in consumeTopic(stoppingToken)
            from _1 in value.Message.Value is null
                ? removePet(value)
                : updatePet(value)
            from _2 in storeOffset(value)
            select unit);
        var operation = retryUntil(
            processStream,
            e => e.Exception.Match(
                Some: ex => ex is OperationCanceledException, 
                None: false));

        Consumer.Subscribe(KafkaTopics.Pets);
        var runtime = new LiveRuntime(Consumer, Redis);
        operation.RunUnit(runtime);
    }
}
