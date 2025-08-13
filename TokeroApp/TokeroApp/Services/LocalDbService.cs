using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokeroApp.Model;

namespace TokeroApp.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "crypto_db.db3";
        private const string CLONE_DB = "clone_db.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {   

            // use clone db if it exists, otherwise create a new one
            // beware that if creating a new db, you have to make an API key to populate it
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            if (!File.Exists(dbPath))
            {
                string sampleDbPath = Path.Combine(AppContext.BaseDirectory, CLONE_DB);

                if (File.Exists(sampleDbPath))
                {
                    File.Copy(sampleDbPath, dbPath);
                    Debug.WriteLine("Clone DB copied to local app data directory.");
                }
                else
                {
                    Debug.WriteLine("No sample DB found — creating new database.");
                }
            }

            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<CryptoCoin>();
        }

        public async Task<List<CryptoCoin>> GetCryptoCoins()
        {
            return await _connection.Table<CryptoCoin>().ToListAsync();
        }

        public async Task<List<CryptoCoin>> GetBySymbol(string symbol)
        {
            return await _connection.Table<CryptoCoin>()
                .Where(p => p.Symbol == symbol)
                .ToListAsync();
        }

        public async Task Create(CryptoCoin coin)
        {
            await _connection.InsertAsync(coin);
        }

        public async Task<bool> PriceExistsAsync(string symbol, DateTime date)
        {
            var record = await _connection.Table<CryptoCoin>()
                .Where(p => p.Symbol == symbol && p.Date == date)
                .FirstOrDefaultAsync();

            return record != null;
        }

        public async Task<decimal?> GetLatestPriceAsync(string symbol, DateTime date)
        {
            var record = await _connection.Table<CryptoCoin>()
                .Where(p => p.Symbol == symbol && p.Date <= date)
                .OrderByDescending(p => p.Date)
                .FirstOrDefaultAsync();

            return record?.PriceEur;
        }

        //get the lsit of distinct coins to populate the selection list in the UI
        public async Task<List<CryptoSelection>> GetDistinctCoinsAsync()
        {
            var allPrices = await _connection.Table<CryptoCoin>().ToListAsync();
            return allPrices
                .Select(p => p.Symbol)
                .Distinct()
                .Select(symbol => new CryptoSelection
                {
                    Id = symbol,
                    Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(symbol),
                    IsSelected = false
                })
                .ToList();
        }
    }
}
