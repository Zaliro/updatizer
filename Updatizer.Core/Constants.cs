using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updatizer.Core
{
    public static class Constants
    {
        // Configuration
        public const string VERSION = "1.0.0.0";
        public const int MIN_UPDATE_INTERVAL = 10;
        public const string HOLDINGS_FILE_NAME = "holdings.json";

        // API Endpoints - Fixer.io
        public const string FIXER_BASE_URL = "http://api.fixer.io";
        public const string FIXER_GET_LATEST = "/latest?base={0}&symbols={1}";

        // API Endpoints - Bittrex.com
        public const string BITTREX_BASE_URL = "https://bittrex.com/api/v1.1";
        public const string BITTREX_GET_MARKET_SUMMARY = "/public/getmarketsummary?market=BTC-{0}";
        public const string BITTREX_GET_USDT_BTC_SUMMARY = "public/getmarketsummary?market=USDT-BTC";
    }
}
