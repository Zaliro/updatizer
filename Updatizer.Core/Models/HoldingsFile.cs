using Updatizer.Core.Enums;
using Updatizer.Core.Models.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updatizer.Core.Models
{
    public class HoldingsFile
    {
        public int UpdateInterval { get; set; }
        public RealCurrenciesEnum TargetCurrency { get; set; }
        public List<WalletEntry> Wallets { get; set; }
    }
}
