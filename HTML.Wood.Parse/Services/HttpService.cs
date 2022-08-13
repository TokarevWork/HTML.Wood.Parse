using System;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HTML.Wood.Parse.Extensions;

namespace HTML.Wood.Parse.Services
{
    internal class HttpService : IDisposable
    {
        private readonly HttpClientHandler _httpClientHandler;
        private readonly HttpClient _httpClient;
        private readonly HttpRequestMessage _httpRequestMessage;

        public HttpService(string adress)
        {
            _httpClientHandler = new HttpClientHandler() { UseDefaultCredentials = true };
            _httpClient = new HttpClient(_httpClientHandler);
            _httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, adress);

            _httpRequestMessage.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml,application/json");
            _httpRequestMessage.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip");
            _httpRequestMessage.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            _httpRequestMessage.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
        }

        public async Task<TResult> GetRequest<TResult>(string requestQuery)
        {
            _httpRequestMessage.Content = new StringContent(requestQuery, Encoding.UTF8, "application/json");
            using (var response = await _httpClient.SendAsync(_httpRequestMessage).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                {
                    return decompressedStream.Deserialize<TResult>();
                }
            }
        }

        public void Dispose()
        {
            _httpClientHandler?.Dispose();
            _httpClient?.Dispose();
            _httpRequestMessage?.Dispose();
        }
    }
}
