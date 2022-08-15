using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client
{
    public class MoviesClient
    {
        public MoviesClient(HttpClient client)
        {
            Client = client;
            Client.BaseAddress = new Uri("http://localhost:57863");
            Client.Timeout = new TimeSpan(0, 0, 30);
            Client.DefaultRequestHeaders.Clear();
        }

        public HttpClient Client { get; }
    }
}
