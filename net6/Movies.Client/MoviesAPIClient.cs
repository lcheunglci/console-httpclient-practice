namespace Movies.Client
{
    public class MoviesAPIClient
    {
        public HttpClient Client { get; set; }

        public MoviesAPIClient(HttpClient client)
        {
            Client = client;
        }
    }
}
