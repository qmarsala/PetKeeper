using Confluent.Kafka;
using LanguageExt;
using static LanguageExt.Prelude;

namespace PetKeeper.Infrastructure;

// an attempt at functional kafka

public interface KafkaIO
{
    ConsumeResult<string, string> Consume(CancellationToken cancellationToken);
    Unit StoreOffset(ConsumeResult<string, string> result);
}

public interface HasKafka<RT>
    where RT : struct, HasKafka<RT>
{
    Eff<RT, KafkaIO> KafkaEff { get; }
}

//maybe?
// how are we supposed to capture this IO?
public struct LiveKafkaIO : KafkaIO
{
    private IConsumer<string, string> Consumer;

    public LiveKafkaIO(IConsumer<string, string> consumer)
    {
        Consumer = consumer;
    }

    public static KafkaIO Create(IConsumer<string, string> consumer)
    {
        return new LiveKafkaIO(consumer);
    }

    public ConsumeResult<string, string> Consume(CancellationToken cancellationToken) =>
        Consumer.Consume(cancellationToken);

    public Unit StoreOffset(ConsumeResult<string, string> result)
    {

        Consumer.StoreOffset(result);
        return unit;
    }
}

public static class Kafka<RT>
    where RT : struct, HasKafka<RT>
{
    public static Eff<RT, ConsumeResult<string, string>> consumeTopic(CancellationToken cancellationToken) 
        => default(RT).KafkaEff.Map(k => k.Consume(cancellationToken));

    public static Eff<RT, Unit> storeOffset(ConsumeResult<string, string> result)
        => default(RT).KafkaEff.Map(k => k.StoreOffset(result));
}
