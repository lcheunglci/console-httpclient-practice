using Movies.Client.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Movies.Client.Services
{
    public class StreamService : IIntegrationService
    {

        private static HttpClient _httpClient = new HttpClient();

        public StreamService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            // await GetPosterWithStream();
            // await GetPosterWithStreamAndCompletionMode();
            await TestGetPosterWithoutStream();
            await TestGetPosterWithStream();
            await TestGetPosterWithStreamAndCompletionMode();
        }

        private async Task GetPosterWithStream()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                response.EnsureSuccessStatusCode();

                var poster = stream.ReadAndDeserializeFromJson<Poster>();

                //using (var streamReader = new StreamReader(stream))
                //{
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);

                //        // do something with the poster
                //    }
                //}
            }
        }

        private async Task GetPosterWithStreamAndCompletionMode()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                response.EnsureSuccessStatusCode();

                var poster = stream.ReadAndDeserializeFromJson<Poster>();
            }
        }

        private async Task GetPosterWithoutStream()
        {
            var request = new HttpRequestMessage(
               HttpMethod.Get,
               $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var poster = JsonConvert.DeserializeObject<Poster>(content);
        }

        public async Task TestGetPosterWithoutStream()
        {
            // warm up
            await GetPosterWithoutStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithoutStream();
            }

            // stop stopwatch
            stopWatch.Stop();

            Console.WriteLine($"Elapsed milliseconds without stream: "
                + $"{stopWatch.ElapsedMilliseconds}, "
                + $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request" 
                );
        }

        public async Task TestGetPosterWithStream()
        {
            // warm up
            await GetPosterWithStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithStream();
            }

            // stop stopwatch
            stopWatch.Stop();

            Console.WriteLine($"Elapsed milliseconds with stream: "
                + $"{stopWatch.ElapsedMilliseconds}, "
                + $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request"
                );
        }

        public async Task TestGetPosterWithStreamAndCompletionMode()
        {
            // warm up
            await GetPosterWithStreamAndCompletionMode();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithStreamAndCompletionMode();
            }

            // stop stopwatch
            stopWatch.Stop();

            Console.WriteLine($"Elapsed milliseconds with stream and completionmode: "
                + $"{stopWatch.ElapsedMilliseconds}, "
                + $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request"
                );
        }


    }
}
