using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Security.AccessControl;
using System.Text.Json;

namespace Movies.Client.Services;

public class HttpClientFactorySamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;
    private readonly MoviesAPIClient _movieAPIClient;

    public HttpClientFactorySamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper, MoviesAPIClient movieAPIClient)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
        _movieAPIClient = movieAPIClient ?? throw new ArgumentNullException(nameof(movieAPIClient));
    }

    public async Task RunAsync()
    {
        // await TestDisposeHttpClientAsync();
        // await TestReuseDisposeHttpClientAsync();
        // await GetFilmsAsync();
        await GetMoviesWithTypedHttpClientAsync();
    }

    private async Task TestDisposeHttpClientAsync()
    {
        for (var i = 0; i < 10; i++)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://www.google.com");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Request completed with status code " + $"{response.StatusCode}");
                // may lead to socket exhausting, sockets remain for up to 240 seconds
            }
        }

        // To verify using command prompt in admin mode run netstat -abn
    }

    private async Task TestReuseDisposeHttpClientAsync()
    {
        var client = new HttpClient();
        for (var i = 0; i < 10; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://www.google.com");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Request completed with status code " + $"{response.StatusCode}");
        }

        // wait 250 seconds if you ran the other test prior
    }

    public async Task GetFilmsAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(HttpMethod.Get,
            "api/films");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
    }

    // Disabled as the refactoring made Client private
    //private async Task GetMoviesWithTypedHttpClientAsync()
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Get,
    //        "api/films");
    //    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    //    var response = await _movieAPIClient.Client.SendAsync(request);
    //    response.EnsureSuccessStatusCode();

    //    var content = await response.Content.ReadAsStringAsync();

    //    var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
    //}

    private async Task GetMoviesBiaMoviesAPIClientAsync()
    {
        var movies = await _movieAPIClient.GetMoviesAsync();
    }

}
