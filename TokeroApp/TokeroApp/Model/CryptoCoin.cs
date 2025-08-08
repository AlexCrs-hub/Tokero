using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroApp.Model
{
    [Table("crypto_coin")]
    public class CryptoCoin
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal PriceUsd { get; set; }
    }
}
