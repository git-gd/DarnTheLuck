using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DarnTheLuck.Helpers
{
    internal class Value
    {
        public int id { get; set; }
        public string joke { get; set; }
    }

    internal class Root
    {
        public string type { get; set; }
        public Value value { get; set; }
    }

    public class Chuck
    {
        public string Joke { get; private set; }
        public Chuck()
        {
            Joke = GetJoke().Result;
        }

        private async Task<String> GetJoke()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://api.icndb.com/jokes/random"),
                Headers =
                {
                    { "accept", "application/json" }
                },
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(body);

            return myDeserializedClass.value.joke.Replace("&quot;", "\""); // remove encoding
        }
    }
}

//http://www.icndb.com/api/