using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokeroApp.Model;

namespace TokeroApp.Services
{
    public class PriceFetcher
    {
        private readonly CryptoPriceService _cryptoPriceService;
        private readonly LocalDbService _localDbService;

        public PriceFetcher(CryptoPriceService cryptoPriceService, LocalDbService localDbService)
        {
            _cryptoPriceService = cryptoPriceService;
            _localDbService = localDbService;
        }   

        // function to populate teh db with the API data on the 15th 20th and 25th of each month
        public async Task FetchAndStoreAsync()
        {
            var topCoinIds = await _cryptoPriceService.GetTopCoinIdsAsync();
            var targetDays = new[] { 15, 20, 25 };

            if (topCoinIds == null || !topCoinIds.Any())
            {
                return; // exit early
            }

            // need to do this since the API provides data within one year
            DateTime start = DateTime.Today.AddDays(-365);
            DateTime end = DateTime.Today;

            start = new DateTime(start.Year, start.Month, targetDays.Min());

            for (DateTime month = start; month <= end; month = month.AddMonths(1))
            {
                foreach (var day in targetDays)
                {
                    DateTime targetDate;
                    try
                    {
                        targetDate = new DateTime(month.Year, month.Month, day);
                    }
                    catch
                    {
                        continue;
                    }

                    if (targetDate < DateTime.Today.AddDays(-365))
                        continue;

                    foreach ( var coinId in topCoinIds)
                    {
                        // checks if the data exists such that it does not update the db with duplicates at new runs
                        bool exists = await _localDbService.PriceExistsAsync(coinId, targetDate);
                        if (exists)
                        {
                            Debug.WriteLine($"Record already exists for coin {coinId} and date {targetDate::dd-MM-yyyy} , skipping.");
                            continue;
                        }

                        // populating the database with the prices from the API
                        var price = await _cryptoPriceService.GetHistoricalPricesAsync(coinId, targetDate);
                        if (price.HasValue)
                        {
                            var cryptoCoin = new CryptoCoin
                            {
                                Symbol = coinId,
                                Date = targetDate,
                                PriceEur = price.Value
                            };

                            await _localDbService.Create(cryptoCoin);
                            Debug.WriteLine($"Saved: {coinId} | {targetDate:yyyy-MM-dd} => €{price.Value:F2}");
                        }

                        // to respect the rate limit of the CoinGecko API
                        await Task.Delay(1500);
                    }
                }
            }
        }
    }
}
