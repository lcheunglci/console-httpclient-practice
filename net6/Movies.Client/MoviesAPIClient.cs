using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

namespace Movies.Client
{
    public class MoviesAPIClient
    {
        private HttpClient Client { get; set; }
        private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

        public MoviesAPIClient(HttpClient client, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
        {
            Client = client;
            Client.BaseAddress = new Uri("http://localhost:5001");
            Client.Timeout = new TimeSpan(0, 0, 30);
            _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
        }

        public async Task<IEnumerable<Movie>?> GetMoviesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<IEnumerable<Movie>>(content,
                _jsonSerializerOptionsWrapper.Options);

        }
    }
}
