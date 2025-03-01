using System.Threading.RateLimiting;
using Polly;

namespace ClientSideRateLimiting
{
    internal class RateLimitingClient
    {
        private readonly HttpClient client;
        private readonly string url;
        private readonly ResiliencePipeline pipeline;

        public RateLimitingClient(string url, int maxRequestsPerSecond)
        {
            client = new HttpClient();
            this.url = url;
            pipeline = new ResiliencePipelineBuilder()
                .AddRateLimiter(
                    new SlidingWindowRateLimiter(
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = maxRequestsPerSecond,
                            SegmentsPerWindow = 10,
                            Window = TimeSpan.FromSeconds(1)
                        }
                    )
                )
                .Build();
        }

        public async Task<string> GetAsync()
        {
            return await pipeline.ExecuteAsync(async cancellationToken =>
                await client.GetStringAsync(url, cancellationToken)
            );
        }
    }
}
