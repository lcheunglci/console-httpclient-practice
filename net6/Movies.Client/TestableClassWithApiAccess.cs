using Movies.Client.Helpers;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using Movies.Client.Models;

namespace Movies.Client
{
    public class TestableClassWithApiAccess
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

        public TestableClassWithApiAccess(HttpClient httpClient, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
        {
            _httpClient = httpClient;
            _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper;
        }

        public async Task<Movie?> GetMovieAsync(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // show this to the user
                        Console.WriteLine("The request movie cannot be found.");
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // trigger a login flow
                        throw new UnauthorizedApiAccessException();
                    }

                    // not using a try catch here has more overhead, this way is safer
                    response.EnsureSuccessStatusCode();
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = await JsonSerializer.DeserializeAsync<Movie>(
                    stream,
                    _jsonSerializerOptionsWrapper.Options);
            }

            return null;
        }

    }
}
