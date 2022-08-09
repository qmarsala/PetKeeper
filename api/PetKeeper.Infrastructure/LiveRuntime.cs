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

    public Eff<LiveRuntime, KafkaIO> KafkaEff => SuccessEff(LiveKafkaIO.Create(Consumer));
    public Eff<LiveRuntime, RedisIO> RedisEff => SuccessEff(LiveRedisIO.Create(ConnectionMultiplexor));
}