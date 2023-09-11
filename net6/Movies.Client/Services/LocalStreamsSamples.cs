using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class LocalStreamsSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public LocalStreamsSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task RunAsync()
    {
        // await GetPosterWithStreamAsync();
        // await GetPosterWithStreamAndCompletionModeAsync();

        await TestMethodAsync(() => GetPosterWithoutStreamAsync());
        await TestMethodAsync(() => GetPosterWithStreamAsync());
        await TestMethodAsync(() => GetPosterWithStreamAndCompletionModeAsync());
    }

    private async Task GetPosterWithStreamAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using (var response = await httpClient.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var poster = await JsonSerializer.DeserializeAsync<Poster>(
                stream,
                _jsonSerializerOptionsWrapper.Options);
        }
    }

    private async Task GetPosterWithStreamAndCompletionModeAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var poster = await JsonSerializer.DeserializeAsync<Poster>(
                stream,
                _jsonSerializerOptionsWrapper.Options);
        }
    }

    private async Task GetPosterWithoutStreamAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using (var response = await httpClient.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var poster = JsonSerializer.Deserialize<Poster>(
                content,
                _jsonSerializerOptionsWrapper.Options);
        }
    }

    public async Task TestMethodAsync(Func<Task> functionToTest)
    {
        // warm up
        await functionToTest();

        // start stopwatch
        var stopWatch = Stopwatch.StartNew();

        // run requests
        for (int i = 0; i < 200; i++)
        {
            await functionToTest();
        }

        // stop stopwatch
        stopWatch.Stop();

        Console.WriteLine($"Elapsed milliseconds without stream: {stopWatch.ElapsedMilliseconds}, "
            + $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
    }

}
