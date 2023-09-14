using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class RemoteStreamingSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public RemoteStreamingSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionWrapper));
    }

    public async Task RunAsync()
    {
        await GetStreamingMoviesAsync();
    }

    private async Task GetStreamingMoviesAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/moviestream");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        
        response.EnsureSuccessStatusCode();

        // regular deserialization
        //var content= await response.Content.ReadAsStringAsync();
        //var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(
        //    content,
        //    _jsonSerializerOptionsWrapper.Options);

        var responseStream = await response.Content.ReadAsStreamAsync();
        var movies = JsonSerializer.DeserializeAsyncEnumerable<Movie>(responseStream, _jsonSerializerOptionsWrapper.Options);

        //foreach (var movie in movies)
        //{
        //    Console.WriteLine(movie?.Title);
        //}

        await foreach (var movie in movies)
        {
            Console.WriteLine(movie?.Title);
        }



    }
}
