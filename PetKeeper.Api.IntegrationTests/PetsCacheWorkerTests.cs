using Microsoft.Extensions.DependencyInjection;
using PetKeeper.Core;
using PetKeeper.Core.Interfaces;
using StackExchange.Redis;

namespace PetKeeper.Api.IntegrationTests
{
    public class PetsCacheWorkerTests
    {
        private int TestTimeoutSeconds = 45;

        [Fact]
        public async Task WhenANewPetIsInTheLog()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {

                });

            using var scope = application.Services.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IWritePets>();
            var newPet = new Pet { Id = "testing-new-pet", Name = "Mooky" };
            await producer.WritePet(newPet);

            var redis = application.Services.GetRequiredService<IConnectionMultiplexer>();
            var db = redis.GetDatabase();

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(TestTimeoutSeconds));
            var token = cancellationSource.Token;

            while (!token.IsCancellationRequested)
            {
                var value = await db.StringGetAsync(newPet.Id);
                if (value.HasValue)
                {
                    ((string)value).ShouldContain(newPet.Name);
                    return;
                }
                await Task.Delay(500);
            }

            false.ShouldBeTrue("never saw value in redis");
        }

        [Fact]
        public async Task WhenAnUpdatedPetIsInTheLog()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {

                });

            using var scope = application.Services.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IWritePets>();
            var newPet = new Pet { Id = "testing-update-pet", Name = "OldValue" };
            await producer.WritePet(newPet);
           
            var updatedPet = newPet with { Name = "NewValue" };
            await producer.WritePet(updatedPet);

            var redis = application.Services.GetRequiredService<IConnectionMultiplexer>();
            var db = redis.GetDatabase();

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(TestTimeoutSeconds));
            var token = cancellationSource.Token;
            while (!token.IsCancellationRequested)
            {
                var value = await db.StringGetAsync(newPet.Id);
                if (value.HasValue)
                {
                    if (((string)value).Contains(updatedPet.Name))
                    {
                        return;
                    }
                }
                await Task.Delay(500);
            }

            false.ShouldBeTrue("never saw updated value in redis");
        }

        [Fact]
        public async Task WhenAPetIsTombstoned()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {

                });

            using var scope = application.Services.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IWritePets>();
            var sawValue = false;
            var redis = application.Services.GetRequiredService<IConnectionMultiplexer>();
            var db = redis.GetDatabase();

            var newPet = new Pet { Id = "testing-tombstone-pet", Name = "OldValue" };
            await producer.WritePet(newPet);

            var seedCancellationSource = new CancellationTokenSource();
            seedCancellationSource.CancelAfter(TimeSpan.FromSeconds(TestTimeoutSeconds));
            var token = seedCancellationSource.Token;
            while (!token.IsCancellationRequested)
            {
                var value = await db.StringGetAsync(newPet.Id);
                if (value.HasValue)
                {
                    if (((string)value).Contains(newPet.Name))
                    {
                        sawValue = true;
                        return;
                    }
                }
                await Task.Delay(500);
            }

            sawValue.ShouldBeTrue("never saw initial value");

            await producer.WritePet(null);

            var tombstoneCancellationSource = new CancellationTokenSource();
            seedCancellationSource.CancelAfter(TimeSpan.FromSeconds(TestTimeoutSeconds));
            var tombstoneToken = seedCancellationSource.Token;
            while (!tombstoneToken.IsCancellationRequested)
            {
                var value = await db.StringGetAsync(newPet.Id);
                if (value.IsNull)
                {
                    sawValue = false;
                    return;
                }
                await Task.Delay(500);
            }

            sawValue.ShouldBeFalse("never saw value leave redis");
        }
    }
}
