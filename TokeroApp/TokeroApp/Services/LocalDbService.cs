using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokeroApp.Model;

namespace TokeroApp.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "crypto_db.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
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

        public async Task<decimal?> GetPriceForDateAsync(string symbol, DateTime date)
        {
            var record = await _connection.Table<CryptoCoin>()
               .Where(p => p.Symbol == symbol && p.Date.Date == date)
               .FirstOrDefaultAsync();

            return record?.PriceEur;
        }

        public async Task<decimal?> GetLatestPriceAsync(string symbol, DateTime date)
        {
            var record = await _connection.Table<CryptoCoin>()
                .Where(p => p.Symbol == symbol && p.Date <= date)
                .OrderByDescending(p => p.Date)
                .FirstOrDefaultAsync();

            return record?.PriceEur;
        }
    }
}
