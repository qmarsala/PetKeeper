namespace PetKeeper.Api.IntegrationTests;

public class ActivityLogCacheWorkerTests
{
    private int TestTimeoutSeconds = 45;

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
        cancellationSource.CancelAfter(TimeSpan.FromSeconds(TestTimeoutSeconds));
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
