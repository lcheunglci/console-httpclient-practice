using Microsoft.AspNetCore.Mvc;
using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class FaultsAndErrorsSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;
    public FaultsAndErrorsSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        await GetMovieAndDealWithInvalidResponseAsync(CancellationToken.None);
        // await PostMovieAndHandleError(CancellationToken.None);
    }

    private async Task GetMovieAndDealWithInvalidResponseAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

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

    private async Task PostMovieAndHandleError(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        var movieForCreation = new MovieForCreation()
        {
            Title = "Pulp Fiction",
            Description = "The movie with Zed.",
            DirectorId = Guid.Parse("d28888e9-2ba9-437a-a40f-e38cb54f9b35"),
            ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
            Genre = "Crime, Drama"
        };

        var serializedMovieForCreation = JsonSerializer.Serialize(movieForCreation, _jsonSerializerOptionsWrapper.Options);

        using (var request = new HttpRequestMessage(HttpMethod.Post, "api/movies"))
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Content = new StringContent(serializedMovieForCreation);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {

                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        // show this to the user
                        // Console.WriteLine("The request movie cannot be found.");

                        var errorStream = await response.Content.ReadAsStreamAsync();


                        var errorAsProblemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(errorStream, _jsonSerializerOptionsWrapper.Options);



                        // var errors = errorAsProblemDetails?.Errors;
                        // 
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
                var movie = await JsonSerializer.DeserializeAsync<Movie>(stream, _jsonSerializerOptionsWrapper.Options);
            }
        }
           
    }

} 