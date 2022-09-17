using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Fcl.Net.Core.Service
{
    public class FetchService
    {
        private readonly HttpClient _httpClient;
        private readonly FetchServiceConfig _fetchServiceConfig;

        public FetchService(HttpClient httpClient, FetchServiceConfig fetchServiceConfig)
        {
            _httpClient = httpClient;
            _fetchServiceConfig = fetchServiceConfig;
        }

        public Uri BuildUrl(FclService service)
        {
            var uriBuilder = new UriBuilder(service.Endpoint);
            var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (!string.IsNullOrWhiteSpace(_fetchServiceConfig.Location))
                paramValues["l6n"] = _fetchServiceConfig.Location;

            foreach (var param in service.Params)
                paramValues[param.Key] = param.Value;

            foreach(var param in _fetchServiceConfig.Params)
                paramValues[param.Key] = param.Value;

            uriBuilder.Query = paramValues.ToString();

            return uriBuilder.Uri;
        }

        public async Task<HttpResponseMessage> FetchAsync(FclService service, Dictionary<string, object> data = null, HttpMethod httpMethod = null)
        {
            if (httpMethod == null)
                httpMethod = HttpMethod.Post;

            var msg = new HttpRequestMessage(httpMethod, BuildUrl(service));

            if (httpMethod != HttpMethod.Get)
                msg.Content = new StringContent(JsonConvert.SerializeObject(data ?? service.Data), Encoding.UTF8, "application/json");

            return await _httpClient.SendAsync(msg).ConfigureAwait(false);
        }

        public async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            if (response == null || response.Content == null)
                throw new FclException("Response was empty");

            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var streamReader = new System.IO.StreamReader(responseStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = JsonSerializer.Create();
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new FclException(message, exception);
            }
        }

        public async Task<T> FetchAndReadResponseAsync<T>(FclService service, Dictionary<string, object> data = null, HttpMethod httpMethod = null)
        {
            var response = await FetchAsync(service, data, httpMethod).ConfigureAwait(false);
            return await ReadResponseAsync<T>(response).ConfigureAwait(false);
        }
    }

    public class FetchServiceConfig
    {
        public FetchServiceConfig()
        {
            Params = new Dictionary<string, string>();
        }

        public string Location { get; set; }
        public Dictionary<string, string> Params { get; set; }
    }
}
