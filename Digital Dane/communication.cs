using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Dane
{
    public class Communication
    {
        private readonly HttpClient httpClient = new();

        /// <summary>
        /// Makes a GET request to the specified endpoint and returns the response as a string.
        /// </summary>
        private string MakeGetCall(string endpoint)
        {
            HttpResponseMessage response = httpClient.GetAsync(endpoint).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API call to {endpoint} failed with status code {response.StatusCode}");
            }

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Makes a POST request to the specified endpoint with the provided data and returns the response as a string.
        /// </summary>
        /// <param name="endpoint">The API endpoint to call.</param>
        /// <param name="data">The data to send in the POST request body (optional).</param>
        /// <returns>The response content as a string.</returns>
        private string MakePostCall(string endpoint)
        {
            StringContent content = new(string.Empty);

            HttpResponseMessage response = httpClient.PostAsync(endpoint, content).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API call to {endpoint} failed with status code {response.StatusCode}");
            }

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

    }
}
