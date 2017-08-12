using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updatizer.Core.Models.Entries
{
    public class WalletEntry
    {
        public string Label { get; set; }
        public List<CurrencyEntry> Holdings { get; set; }

        [JsonIgnore]
        public double LastBTCEstimate { get; set; }
        [JsonIgnore]
        public double LastRealEstimate { get; set; }
        [JsonIgnore]
        public bool MissingCurrencies { get; set; }
    }
}
