using Movies.Client;

namespace Movies.UnitTests
{
    public class TestableClassWithApiAccessUnitTests
    {
        [Fact]
        public async void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException()
        {
            var httpClient = new HttpClient(new Return401UnauthorizeResponseHandler())
            {
                BaseAddress = new Uri("http://localhost:57863")
            };

            var testableClass = new TestableClassWithApiAccess(httpClient);

            await Assert.ThrowsAsync<UnauthorizedApiAccessException>
                (() => testableClass.GetMovie(CancellationToken.None));
        }
    }
}