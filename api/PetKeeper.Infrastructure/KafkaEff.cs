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

public struct LiveRuntime : HasKafka<LiveRuntime>
{
    private IConsumer<string, string> Consumer;

    public LiveRuntime(IConsumer<string, string> consumer)
    {
        Consumer = consumer;
    }

    public Eff<LiveRuntime, KafkaIO> KafkaEff => SuccessEff(LiveKafkaIO.Create(Consumer));
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

// this seemed simpler - but the runtime is supposed to be cool?
public static class KafkaEff
{
    public static Eff<ConsumeResult<string, string>> consumeTopic(IConsumer<string, string> consumer) =>
        Eff(() => consumer.Consume());

    public static Eff<Unit> storeOffset(IConsumer<string, string> consumer, ConsumeResult<string, string> result) =>
        Eff(() =>
        {
            consumer.StoreOffset(result);
            return unit;
        });
}
