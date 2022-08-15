﻿using Marvin.StreamExtensions;
using Movies.Client.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Movies.Client.Services
{
    public class CancellationService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient(
           new HttpClientHandler()
           {
               AutomaticDecompression = System.Net.DecompressionMethods.GZip
           });

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        public CancellationService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            _cancellationTokenSource.CancelAfter(1000);
            await GetTrailerAndCancel(_cancellationTokenSource.Token);
        }

        private async Task GetTrailerAndCancel(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            

            using (var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    response.EnsureSuccessStatusCode();

                    var poster = stream.ReadAndDeserializeFromJson<Trailer>();
                }
            }
        }

    }
}
