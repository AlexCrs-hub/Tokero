using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace TokeroApp.Services
{
    public class CryptoPriceService
    {
        private readonly HttpClient _client = new();
        private readonly string _apiKey = "";

        public CryptoPriceService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("x-cg-demo-api-key", _apiKey);
        }

        public async Task<List<String>> GetTopCoinIdsAsync(int limit = 10)
        {
            var url = $"coins/markets?vs_currency=eur&order=market_cap_desc&per_page={limit}&page=1";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JArray.Parse(json);

            return data.Select(c => c["id"]!.ToString()).Take(limit).ToList();
        }

        public async Task<decimal?> GetHistoricalPricesAsync(string coinId, DateTime date)
        {
            string formattedDate = date.ToString("dd-MM-yyyy");
            var url = $"coins/{coinId}/history?date={formattedDate}&localization=false";
            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
             
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);
                var price = data["market_data"]?["current_price"]?["eur"]?.Value<decimal>();

                Debug.WriteLine(data);

                return price;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[CoinGeckoService] Error fetching price for {coinId} on {date:yyyy-MM-dd}: {ex.Message}");
                return null; // could be rate limit or coin not found on the specified date
            }

        }
    }
}
