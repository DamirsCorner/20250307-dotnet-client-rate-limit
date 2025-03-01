using Polly.RateLimiting;
using Shouldly;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ClientSideRateLimiting
{
    public class RateLimitingClientTests
    {
        private WireMockServer server;

        [SetUp]
        public void Setup()
        {
            server = WireMockServer.Start();
            server
                .Given(Request.Create().WithPath("/").UsingGet())
                .RespondWith(Response.Create().WithBody("Hello world!"));
        }

        [TearDown]
        public void TearDown()
        {
            server.Stop();
            server.Dispose();
        }

        [Test]
        public async Task BlocksCallsExceedingRateLimit()
        {
            var client = new RateLimitingClient(server.Urls[0], 5);

            var tasks = Enumerable.Range(0, 5).Select(_ => client.GetAsync()).ToArray();
            await Task.WhenAll(tasks).ShouldNotThrowAsync();

            await client.GetAsync().ShouldThrowAsync<RateLimiterRejectedException>();

            await Task.Delay(TimeSpan.FromSeconds(1));
            await client.GetAsync().ShouldNotThrowAsync();
        }
    }
}
