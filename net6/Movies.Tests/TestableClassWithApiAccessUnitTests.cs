using Movies.Client;
using Movies.Client.Handlers;

namespace Movies.Tests
{
    public class TestableClassWithApiAccessUnitTests
    {
        [Fact]
        public async void GetMovie_on401Response_MustThrowUnauthhorizedApiAccessException()
        {
            var httpClient = new HttpClient(new Return401UnauthorizedResponseHandler())
            {
                BaseAddress = new Uri("http://localhost:5001")
            };

            var testableClass = new TestableClassWithApiAccess(
                httpClient,
                new Client.Helpers.JsonSerializerOptionsWrapper());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => testableClass.GetMovieAsync(CancellationToken.None));
        }
    }
}