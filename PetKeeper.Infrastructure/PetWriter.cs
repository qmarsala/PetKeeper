using Confluent.Kafka;
using LanguageExt;
using LanguageExt.Common;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;
using System.Text.Json;

namespace PetKeeper.Infrastructure;

public class PetWriter : IWritePets
{
    public PetWriter(IProducer<string, string> producer)
    {
        Producer = producer;
    }

    public IProducer<string, string> Producer { get; }

    public async Task<Result<Unit>> RemovePet(string petId)
    {
        try
        {
            await Producer.ProduceAsync(KafkaTopics.Pets, new Message<string, string>
            {
                Key = petId,
                Value = null
            });
            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Pet>> WritePet(Pet pet)
    {
        try
        {
            await Producer.ProduceAsync(KafkaTopics.Pets, new Message<string, string>
            {
                Key = pet.Id,
                Value = JsonSerializer.Serialize(pet)
            });
            return pet;
        }
        catch (Exception e)
        {
            return new Result<Pet>(e);
        }
    }
}
