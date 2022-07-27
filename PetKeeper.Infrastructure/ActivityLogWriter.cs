using Confluent.Kafka;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;
using System.Text.Json;

namespace PetKeeper.Infrastructure;

public class ActivityLogWriter : IWriteActivityLogs
{
    public ActivityLogWriter(IProducer<string, string> producer)
    {
        Producer = producer;
    }

    public IProducer<string, string> Producer { get; }

    public async Task<Result<Activity>> WriteActivityLog(Activity newActivity)
    {
        try
        {
            await Producer.ProduceAsync(KafkaTopics.ActivityLog, new Message<string, string>
            {
                Key = newActivity.Id,
                Value = JsonSerializer.Serialize(newActivity)
            });
            return newActivity;
        }
        catch (Exception e)
        {
            return new Result<Activity>(e);
        }
    }
}

