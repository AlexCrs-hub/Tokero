using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroApp.Model
{
    public class CryptoSelection
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public decimal InvestmentAmount { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
    }
}
