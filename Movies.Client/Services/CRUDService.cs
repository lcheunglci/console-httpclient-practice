using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class CRUDService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public CRUDService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
        }

        public async Task Run()
        {
            await GetResource();
        }

        public async Task GetResource()
        {
            // even though it implements IDisposable, the message handler is intented to be long lived and reused.
            //using (var httpClient = new HttpClient())
            //{
            //}

            var response = await _httpClient.GetAsync("api/movies");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content,
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

        }
    }
}