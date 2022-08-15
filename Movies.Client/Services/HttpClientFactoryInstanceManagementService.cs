namespace Movies.Client.Services
{
    public class HttpClientFactoryInstanceManagementService : IIntegrationService
    {
        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();
        public async Task Run()
        {
            // await TestDisposeHttpClient(_cancellationTokenSource.Token);
            await TestReuseHttpClient(_cancellationTokenSource.Token);
        }

        // run admin cmd netstat -abn
        // when the connection is dispose it may remain in this time_wait state for up to 240 seconds, and can lead to socket exhaustion

        private async Task TestDisposeHttpClient(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        "https://www.google.com");

                    using (var response = await httpClient.SendAsync(request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken))
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        response.EnsureSuccessStatusCode();

                        Console.WriteLine($"Request completed with status code {response.StatusCode}");
                    }
                }
            }
        }

        private async Task TestReuseHttpClient(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            for (int i = 0; i < 10; i++)
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://www.google.com");

                using (var response = await httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    response.EnsureSuccessStatusCode();

                    Console.WriteLine($"Request completed with status code {response.StatusCode}");
                }

            }
        }
    }
}
