namespace Movies.Client.Services;

public class HttpClientFactorySamples : IIntegrationService
{
    public async Task RunAsync()
    {
        // await TestDisposeHttpClientAsync();
        await TestReuseDisposeHttpClientAsync();
    }

    private async Task TestDisposeHttpClientAsync()
    {
        for (var i = 0; i < 10; i++)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://www.google.com");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Request completed with status code " + $"{response.StatusCode}");
                // may lead to socket exhausting, sockets remain for up to 240 seconds
            }
        }

        // To verify using command prompt in admin mode run netstat -abn
    }

    private async Task TestReuseDisposeHttpClientAsync()
    {
        var client = new HttpClient();
        for (var i = 0; i < 10; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://www.google.com");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Request completed with status code " + $"{response.StatusCode}");
        }

        // wait 250 seconds if you ran the other test prior
    }
}
