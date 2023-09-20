using Movies.Client.Helpers;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Threading;
using Movies.Client.Models;

namespace Movies.Client.Services;

public class CustomMessageHandlersSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;
    public CustomMessageHandlersSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        await GetMoviesWithCustomRetryHandlerAsync(CancellationToken.None);
    }

    public async Task GetMoviesWithCustomRetryHandlerAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClientWithCustomHandler");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            if (!response.IsSuccessStatusCode)
            {
                // inspect the status code
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // show this to the user
                    Console.WriteLine("The request movie cannot be found.");
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // trigger a login flow
                    return;
                }

                // not using a try catch here has more overhead, this way is safer
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var poster = await JsonSerializer.DeserializeAsync<Movie>(
                stream,
                _jsonSerializerOptionsWrapper.Options);
        }
    }
}
