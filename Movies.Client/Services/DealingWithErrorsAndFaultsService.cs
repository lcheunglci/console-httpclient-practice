using Marvin.StreamExtensions;
using Movies.Client.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

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
            // await GetMovieAndDealWithInvalidResponse(_cancellationTokenSource.Token);
            await PostMovieAndHandleValidationErrors(_cancellationTokenSource.Token);
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

        public async Task PostMovieAndHandleValidationErrors(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");
            var movieToCreate = new MovieForCreation()
            {
                Title = "Pulp FIction"
            };

            var serializeMovieToCreate = JsonSerializer.Serialize(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            request.Content = new StringContent(serializeMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var stream = await response.Content.ReadAsStreamAsync();

            if (!response.IsSuccessStatusCode)
            {
                // inspect the status code
                if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    // read out the respnse body and log it into the console window
                    var validationErrors = stream.ReadAndDeserializeFromJson();
                    Console.WriteLine(validationErrors);
                    return;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // trigger a login flow
                    return;
                }

                response.EnsureSuccessStatusCode();
            }

            var movie = stream.ReadAndDeserializeFromJson<Movie>();
        }


    }
}
