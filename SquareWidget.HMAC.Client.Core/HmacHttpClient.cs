using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SquareWidget.HMAC.Client.Core
{
    public class HmacHttpClient : HttpClient
    {
        private string _hashHeaderName = "Hash";
        private string _timestampHeaderName = "Timestamp";
        private ClientCredentials _clientCredentials;

        public HmacHttpClient() { }

        public HmacHttpClient(
            string baseAddress,
            ClientCredentials credentials,
            string hashRequestHeaderName = "Hash",
            string timestampRequestHeaderName = "Timestamp")
        {
            BaseUrl = baseAddress;
            ClientCredentials = credentials;
            HashHeaderName = hashRequestHeaderName;
            TimestampHeaderName = timestampRequestHeaderName;
        }

        public string BaseUrl
        {
            set
            {
                SetBaseAddress(value);
            }
        }

        public ClientCredentials ClientCredentials
        {
            set
            {
                if (string.IsNullOrEmpty(value.ClientId))
                {
                    throw new ArgumentNullException(nameof(value.ClientId));
                }

                if (string.IsNullOrEmpty(value.ClientSecret))
                {
                    throw new ArgumentNullException(nameof(value.ClientSecret));
                }

                _clientCredentials = value;
            }
        }

        public string HashHeaderName
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _hashHeaderName = value.Trim();
            }
        }

        public string TimestampHeaderName
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _timestampHeaderName = value.Trim();
            }
        }

        /// <summary>
        /// Send an HTTP GET to the service for one or more strongly-typed objects.
        /// Usage: Get<Widget>("api/widget/1") or Get<List<Widget>>("api/widgets")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri">Path to resource</param>
        /// <returns></returns>
        public async Task<T> Get<T>(string requestUri) where T : new()
        {
            InitializeRequest();
            var response = await GetAsync(requestUri);
            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Post a resource. Usage: Post<Widget>("/api/widgets", widget);
        /// </summary>
        /// <typeparam name="T">JSON serializable object</typeparam>
        /// <param name="requestUri">Path to resource</param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<T> Post<T>(string requestUri, T item)
        {
            InitializeRequest();
            var response = await this.PostAsJsonAsync(requestUri, item);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Put resource. Usage: Put<Widget>("/api/widgets", widget);
        /// </summary>
        /// <typeparam name="T">JSON serializable object</typeparam>
        /// <param name="requestUri">Path to resource</param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<T> Put<T>(string requestUri, T item)
        {
            InitializeRequest();
            var response = await this.PutAsJsonAsync(requestUri, item);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Delete a resource. Usage: Delete("/api/widgets/1");
        /// </summary>
        /// <param name="requestUri">Path to resource</param>
        /// <returns></returns>
        public async Task<HttpStatusCode> Delete(string requestUri)
        {
            InitializeRequest();
            var response = await DeleteAsync(requestUri);
            return response.StatusCode;
        }

        /// <summary>
        /// Initialize the request headers with the timestamp and hash values.
        /// </summary>
        public void InitializeRequest()
        {
            if (string.IsNullOrEmpty(_clientCredentials.ClientId))
            {
                throw new ArgumentNullException(nameof(_clientCredentials.ClientId));
            }

            if (string.IsNullOrEmpty(_clientCredentials.ClientSecret))
            {
                throw new ArgumentNullException(nameof(_clientCredentials.ClientSecret));
            }

            var timestamp = DateTime.UtcNow;
            var timestampValue = timestamp.ToString("s", CultureInfo.InvariantCulture);
            var hash = ComputeHash(_clientCredentials.ClientSecret, timestamp);
            var hashPayload = _clientCredentials.ClientId + ":" + hash;
            DefaultRequestHeaders.Clear();
            DefaultRequestHeaders.Add(_timestampHeaderName, timestampValue);
            DefaultRequestHeaders.Add(_hashHeaderName, hashPayload);
            DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        }

        /// <summary>
        /// Sets the base URI for the HttpClient. NOTE: any trailing backslash is stripped off.
        /// </summary>
        /// <param name="baseAddress">https://localhost:12345</param>
        private void SetBaseAddress(string baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            BaseAddress = new Uri(baseAddress.TrimEnd('/'));
        }

        /// <summary>
        /// Computes the HMAC hash given the client secret and UTC timestamp
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private static string ComputeHash(string clientSecret, DateTime timestamp)
        {
            var keyBytes = Encoding.UTF8.GetBytes(clientSecret);
            var ticks = timestamp.Ticks.ToString(CultureInfo.InvariantCulture);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(ticks));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
