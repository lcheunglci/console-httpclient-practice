using Microsoft.AspNetCore.JsonPatch;
using Movies.Client.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Movies.Client.Services;

public class PartialUpdateSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PartialUpdateSamples(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task RunAsync()
    {
        await PatchResourceAsync();
    }

    public async Task PatchResourceAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");
        var patchDoc = new JsonPatchDocument<MovieForUpdate>();
        patchDoc.Replace(m => m.Title, "Update title");
        patchDoc.Remove(m => m.Description);

        var serializedChangeSet = JsonConvert.SerializeObject(patchDoc);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(serializedChangeSet);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var updateMovie = JsonConvert.DeserializeObject<Movie>(content);


    }
}
