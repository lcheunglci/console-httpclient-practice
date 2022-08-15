using Movies.Client.Models;
using System.Net.Http.Headers;
using Marvin.StreamExtensions;

namespace Movies.Client.Services
{
    public class HttpClientFactoryInstanceManagementService : IIntegrationService
    {
        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientFactoryInstanceManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task Run()
        {
            // await TestDisposeHttpClient(_cancellationTokenSource.Token);
            // await TestReuseHttpClient(_cancellationTokenSource.Token);
            // await GetMoviesWithHttpClientFromFactory(_cancellationTokenSource.Token);
            await GetMoviesWithNamedHttpClientFromFactory(_cancellationTokenSource.Token);

        }

        private async Task GetMoviesWithHttpClientFromFactory(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:57863/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }

        private async Task GetMoviesWithNamedHttpClientFromFactory(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
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
