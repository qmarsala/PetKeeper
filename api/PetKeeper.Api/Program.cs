using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using MediatR;
using PetKeeper.Core.Commands;
using Confluent.Kafka;
using PetKeeper.Api;
using StackExchange.Redis;
using PetKeeper.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(typeof(CreateNewPetHandler));
builder.Services.AddSingleton(sp =>
{
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = "localhost",
        Acks = Acks.All
    };
    var producerBuilder = new ProducerBuilder<string, string>(producerConfig);
    return producerBuilder.Build();
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect("localhost");
});
builder.Services.AddScoped<IWritePets, PetWriter>();
builder.Services.AddScoped<IReadPets, PetReader>();
builder.Services.AddScoped<IWriteActivityLogs, ActivityLogWriter>();
builder.Services.AddScoped<IReadActivityLogs, ActivityLogReader>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<PetCacheWorker>(sp =>
{
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "localhost",
        GroupId = "petkeeper-petcache",
        EnableAutoOffsetStore = false,
        AutoOffsetReset = AutoOffsetReset.Earliest
    };
    var consumerBuilder = new ConsumerBuilder<string, string>(consumerConfig);
    var consumer = consumerBuilder.Build();
    return new PetCacheWorker(consumer, sp.GetRequiredService<IConnectionMultiplexer>());
});

builder.Services.AddHostedService<ActivityLogCacheWorker> (sp =>
{
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "localhost",
        GroupId = "petkeeper-activitycache",
        EnableAutoOffsetStore = false,
        AutoOffsetReset = AutoOffsetReset.Earliest
    };
    var consumerBuilder = new ConsumerBuilder<string, string>(consumerConfig);
    var consumer = consumerBuilder.Build();
    return new ActivityLogCacheWorker(consumer, sp.GetRequiredService<IConnectionMultiplexer>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection(); // don't need this yet

app.RegisterPetEndpoints();
app.RegisterAcitivyLogEndpoints();

app.Run();
