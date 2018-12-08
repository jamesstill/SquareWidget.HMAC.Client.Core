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
        public HmacHttpClient(
            string baseAddress, 
            ClientCredentials credentials, 
            string hashRequestHeaderName = "Hash", 
            string timestampRequestHeaderName = "Timestamp")
        {
            var hashHeaderName = hashRequestHeaderName;
            var timestampHeaderName = timestampRequestHeaderName;
            SetBaseAddress(baseAddress);

            var timestamp = DateTime.UtcNow;
            var timestampValue = timestamp.ToString("o", CultureInfo.InvariantCulture);
            var hash = ComputeHash(credentials.ClientSecret, timestamp);
            var hashPayload = credentials.ClientId + ":" + hash;
            DefaultRequestHeaders.Clear();
            DefaultRequestHeaders.Add(timestampHeaderName, timestampValue);
            DefaultRequestHeaders.Add(hashHeaderName, hashPayload);
            DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        }

        /// <summary>
        /// Send an HTTP GET to the service for one or more strongly-typed objects.
        /// Usage: Get<Person>("api/v1/persons/1") or Get<List<Person>>("api/v1/persons")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri">Path to resource</param>
        /// <returns></returns>
        public async Task<T> Get<T>(string requestUri) where T : new()
        {
            var response = await GetAsync(requestUri);
            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Post a resource. Usage: Post<Person>("/api/v1/persons", person);
        /// </summary>
        /// <typeparam name="T">JSON serializable object</typeparam>
        /// <param name="requestUri">Path to resource</param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<T> Post<T>(string requestUri, T item)
        {
            var response = await this.PostAsJsonAsync(requestUri, item);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Put resource. Usage: Put<Person>("/api/v1/persons", person);
        /// </summary>
        /// <typeparam name="T">JSON serializable object</typeparam>
        /// <param name="requestUri">Path to resource</param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<T> Put<T>(string requestUri, T item)
        {
            var response = await this.PutAsJsonAsync(requestUri, item);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Delete a resource. Usage: Delete("/api/v1/persons/1");
        /// </summary>
        /// <param name="requestUri">Path to resource</param>
        /// <returns></returns>
        public async Task<HttpStatusCode> Delete(string requestUri)
        {
            var response = await DeleteAsync(requestUri);
            return response.StatusCode;
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

        /// <summary>
        /// Sets the base URI for the HttpClient. NOTE: trailing backslash is stripped off.
        /// </summary>
        /// <param name="baseAddress">http://localhost:12345</param>
        private void SetBaseAddress(string baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            BaseAddress = new Uri(baseAddress.TrimEnd('/'));
        }
    }
}
