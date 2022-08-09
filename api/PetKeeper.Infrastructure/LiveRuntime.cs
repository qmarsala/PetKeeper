using Confluent.Kafka;
using LanguageExt;
using StackExchange.Redis;
using static LanguageExt.Prelude;

namespace PetKeeper.Infrastructure;

public struct LiveRuntime : HasKafka<LiveRuntime>, HasRedis<LiveRuntime>
{
    private readonly IConsumer<string, string> Consumer;
    private readonly IConnectionMultiplexer ConnectionMultiplexor;

    public LiveRuntime(IConsumer<string, string> consumer, IConnectionMultiplexer connectionMultiplexor)
    {
        Consumer = consumer;
        ConnectionMultiplexor = connectionMultiplexor;
    }

    // static anon func makes sure we don't accidentally capture "this"
    public Eff<LiveRuntime, KafkaIO> KafkaEff => Eff<LiveRuntime, KafkaIO>(static rt => LiveKafkaIO.Create(rt.Consumer));
    public Eff<LiveRuntime, RedisIO> RedisEff => Eff<LiveRuntime, RedisIO>(static rt => LiveRedisIO.Create(rt.ConnectionMultiplexor));
}