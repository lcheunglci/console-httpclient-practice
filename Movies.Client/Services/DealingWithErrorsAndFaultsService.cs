using Marvin.StreamExtensions;
using Movies.Client.Models;
using System.Net.Http.Headers;

namespace Movies.Client.Services
{
    public class DealingWithErrorsAndFaultsService : IIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private MoviesClient _moviesClient;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public DealingWithErrorsAndFaultsService(IHttpClientFactory httpClientFactory,
            MoviesClient moviesClient)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _moviesClient = moviesClient;
        }

        public async Task Run()
        {
            await GetMovieAndDealWithInvalidResponse(_cancellationTokenSource.Token);
        }

        private async Task GetMovieAndDealWithInvalidResponse(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies/030a43b0-f9a5-405-811c-bf342524b2be");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // show this to the user
                        Console.WriteLine("The requested movie cannot be found.");
                        return;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // trigger a login flow
                        return;
                    }

                    response.EnsureSuccessStatusCode();
                }

                var stream = await response.Content.ReadAsStreamAsync();

                var movies = stream.ReadAndDeserializeFromJson<Movie>();
            }
        }
    }
}
