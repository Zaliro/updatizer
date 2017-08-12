using Updatizer.Core.Enums;
using Updatizer.Core.Managers;
using Updatizer.Core.Models.Entries;
using Updatizer.Core.Requests;
using Updatizer.Core.Responses;
using Updatizer.Shared.Reflection;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Updatizer.Core
{
    public class Updatizer : Singleton<Updatizer>
    {
        public Action<UpdatizerStatusEnum> OnStatusChanged { get; set; }
        public Action<RealCurrenciesEnum, List<WalletEntry>> OnWalletsUpdated { get; set; }

        private const string BTC_PRICE_REFERENT = "_BTC";
        private const string BTC_CURRENCY_HEADER = "BTC";
        private const string USD_CURRENCY_HEADER = "USD";

        private Timer updateTimer;

        private RestClient bittrexClient;
        private RestClient fixerClient;

        private double targetCurrencyRate;

        private Dictionary<string, CurrencyEntry> currenciesCache;

        public void Initialize()
        {
            StorageManager.Initiliaze();

            if (StorageManager.HoldingsFile.UpdateInterval >= Constants.MIN_UPDATE_INTERVAL)
            {
                this.updateTimer = new Timer(StorageManager.HoldingsFile.UpdateInterval * 1000);
                this.updateTimer.Elapsed += updateTimer_Elapsed;

                this.bittrexClient = new RestClient(Constants.BITTREX_BASE_URL);
                this.fixerClient = new RestClient(Constants.FIXER_BASE_URL);

                this.registerCurrencies();
            }
            else
            {
                throw new Exception(string.Format("Minimum UpdateInterval is {0} seconds. Thank's for public APIs.", Constants.MIN_UPDATE_INTERVAL));
            }
        }

        public async void Start()
        {
            this.OnStatusChanged?.Invoke(UpdatizerStatusEnum.UPDATING);
            await this.estimateWallets();

            this.OnStatusChanged?.Invoke(UpdatizerStatusEnum.WAITING);
            this.updateTimer.Start();
        }

        public void Stop()
        {
            this.updateTimer.Stop();
            this.OnStatusChanged?.Invoke(UpdatizerStatusEnum.STOPPED);
        }

        #region Internal Methods

        #region Currencies Cache

        private void registerCurrencies()
        {
            this.currenciesCache = new Dictionary<string, CurrencyEntry>();
            this.currenciesCache.Add(BTC_PRICE_REFERENT, new CurrencyEntry() { Name = BTC_PRICE_REFERENT });

            foreach (WalletEntry wallet in StorageManager.HoldingsFile.Wallets)
            {
                foreach(CurrencyEntry currency in wallet.Holdings)
                {
                    if(!this.currenciesCache.ContainsKey(currency.Name))
                    {
                        this.currenciesCache.Add(currency.Name, currency);
                    }
                }
            }
        }

        private bool getCurrencyStatus(string currencyName)
        {
            return this.currenciesCache[currencyName].WasUpdated;
        }

        private void updateCurrencyStatus(string currencyName, bool status)
        {
            this.currenciesCache[currencyName].WasUpdated = status;
        }

        private void resetCurrenciesStatus()
        {
            foreach(var currency in this.currenciesCache.Keys.ToList())
            {
                this.currenciesCache[currency].WasUpdated = false;
            }
        }

        #endregion

        private async Task estimateWallets()
        {
            await Task.Run(async () =>
            {
                // Reseting currencies status...
                this.resetCurrenciesStatus();

                // Retrieving BTC price...
                await this.retrieveBtcPrice();

                // Retrieving TargetCurrency rate...
                if (StorageManager.HoldingsFile.TargetCurrency != RealCurrenciesEnum.USD)
                    await this.retrieveTargetCurrencyRate();
                else
                    this.targetCurrencyRate = 1;

                // Retrieving currencies BTC values...
                foreach (var currency in this.currenciesCache)
                {
                    await this.retrieveCurrencyPrice(currency.Value);
                }

                // Calculating wallets BTC values...
                foreach(var wallet in StorageManager.HoldingsFile.Wallets)
                {
                    wallet.LastBTCEstimate = 0;
                    wallet.MissingCurrencies = false;

                    foreach(var currency in wallet.Holdings)
                    {
                        if (currency.Name != BTC_PRICE_REFERENT || currency.Name != BTC_CURRENCY_HEADER || currency.Value > 0)
                        {
                            double currencyBTCLastEstimate = this.currenciesCache[currency.Name].LastEstimate;
                            currency.LastEstimate = currencyBTCLastEstimate;

                            if (currencyBTCLastEstimate > 0)
                            {
                                double currencyBtcTotal = currencyBTCLastEstimate * currency.Value;
                                wallet.LastBTCEstimate += currencyBtcTotal;
                            }
                            else
                            {
                                wallet.MissingCurrencies = true;
                            }
                        }
                    }

                    // If wallet contains BTC...
                    var walletBTCCurrency = wallet.Holdings.FirstOrDefault(x => x.Name == BTC_CURRENCY_HEADER);
                    if (walletBTCCurrency != null)
                    {
                        wallet.LastBTCEstimate += walletBTCCurrency.Value;
                    }

                    // Calculating wallet value in target currency...
                    double walletUSDEstimate = wallet.LastBTCEstimate * this.currenciesCache[BTC_PRICE_REFERENT].LastEstimate;
                    wallet.LastRealEstimate = walletUSDEstimate * this.targetCurrencyRate;
                }

                this.OnWalletsUpdated(StorageManager.HoldingsFile.TargetCurrency, StorageManager.HoldingsFile.Wallets);
            });
        }

        private async Task retrieveCurrencyPrice(CurrencyEntry currency)
        {
            await Task.Run(async () =>
            {
                if (currency.Name != BTC_PRICE_REFERENT && currency.Name != BTC_CURRENCY_HEADER && !this.getCurrencyStatus(currency.Name))
                {
                    RestRequest getMarketSummaryRequest = new RestRequest(string.Format(Constants.BITTREX_GET_MARKET_SUMMARY, currency.Name), Method.GET);

                    IRestResponse<BittrexResponse> response = await this.bittrexClient.ExecuteGetTaskAsync<BittrexResponse>(getMarketSummaryRequest);
                    if (response.Data.Success)
                    {
                        currency.LastEstimate = (double)((Dictionary<string, object>)response.Data.Result[0])["Last"];
                        this.updateCurrencyStatus(currency.Name, true);
                    }
                    else
                    {
                        currency.LastEstimate = -1;
                        this.updateCurrencyStatus(currency.Name, false);
                    }
                }
            });
        }

        private async Task retrieveBtcPrice()
        {
            await Task.Run(async () =>
            {
                RestRequest getUsdtBtcSummaryRequest = new RestRequest(Constants.BITTREX_GET_USDT_BTC_SUMMARY, Method.GET);

                IRestResponse<BittrexResponse> response = await this.bittrexClient.ExecuteGetTaskAsync<BittrexResponse>(getUsdtBtcSummaryRequest);
                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Data.Success)
                {
                    this.currenciesCache[BTC_PRICE_REFERENT].LastEstimate = (double)((Dictionary<string, object>)response.Data.Result[0])["Last"];
                }
                else
                {
                    throw new Exception(string.Format("Unable to retrieve {0}-BTC value !", USD_CURRENCY_HEADER));
                }
            });
        }

        private async Task retrieveTargetCurrencyRate()
        {
            await Task.Run(async () =>
            {
                RestRequest getLatest = new RestRequest(string.Format(Constants.FIXER_GET_LATEST, USD_CURRENCY_HEADER, StorageManager.HoldingsFile.TargetCurrency), Method.GET);

                IRestResponse<FixerLatestResponse> response = await this.fixerClient.ExecuteGetTaskAsync<FixerLatestResponse>(getLatest);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    this.targetCurrencyRate = response.Data.Rates[StorageManager.HoldingsFile.TargetCurrency.ToString()];
                }
                else
                {
                    throw new Exception(string.Format("Unable to retrieve {0}-{1} rate !", USD_CURRENCY_HEADER, StorageManager.HoldingsFile.TargetCurrency));
                }
            });
        }

        #endregion

        #region Internal Events

        private async void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.OnStatusChanged?.Invoke(UpdatizerStatusEnum.UPDATING);
            this.updateTimer.Stop();

            await this.estimateWallets();

            this.OnStatusChanged?.Invoke(UpdatizerStatusEnum.WAITING);
            this.updateTimer.Start();
        }

        #endregion
    }
}
