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

        public async Task FetchAndStoreAsync()
        {
            var topCoinIds = await _cryptoPriceService.GetTopCoinIdsAsync();
            var targetDays = new[] { 15, 20, 25 };

            if (topCoinIds == null || !topCoinIds.Any())
            {
                return; // Exit early
            }

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

                        bool exists = await _localDbService.PriceExistsAsync(coinId, targetDate);
                        if (exists)
                        {
                            Debug.WriteLine($"Record already exists for coin {coinId} and date {targetDate::dd-MM-yyyy} , skipping.");
                            continue;
                        }

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
