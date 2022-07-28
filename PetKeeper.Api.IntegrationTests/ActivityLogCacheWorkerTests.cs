using Microsoft.Extensions.DependencyInjection;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;
using PetKeeper.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;

namespace PetKeeper.Api.IntegrationTests
{
    public class ActivityLogCacheWorkerTests
    {
        [Fact]
        public async Task WhenANewActivityIsInTheLog()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {

                });

            using var scope = application.Services.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IWriteActivityLogs>();
            var newActivity = new Activity { Id = "testing-new-activity", PetId = "some-pet" };
            await producer.WriteActivityLog(newActivity);

            var redis = application.Services.GetRequiredService<IConnectionMultiplexer>();
            var db = redis.GetDatabase();

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(15));
            var token = cancellationSource.Token;

            var sawMessage = false;
            while (!token.IsCancellationRequested)
            {
                var result = await db.StringGetAsync(newActivity.Id);                
                sawMessage = result.HasValue;
                if (sawMessage) { break; }
                await Task.Delay(500);
            }

            sawMessage.ShouldBeTrue("never saw value in redis");
        }
    }
}
