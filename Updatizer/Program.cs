using Updatizer.Core;
using Updatizer.Core.Enums;
using Updatizer.Core.Models.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Updatizer
{
    class Program
    {
        const int CURRENCIES_PER_LINE = 3;

        static UpdatizerStatusEnum status;
        static int counter = 0;

        static int currenciesInformationsLinesCount = 0;

        static void Main(string[] args)
        {
            Console.Title = string.Format("Updatizer v{0}", Constants.VERSION); ;

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("");
            Console.WriteLine(@"   .-..-..---..--. .---..---..-..---,.---..---. ");
            Console.WriteLine(@"   | || || |-'| \ \| | |`| |'| | / / | |- | |-<  github.com/Zaliro/Updatizer");
            Console.WriteLine(@"   `----'`-'  `-'-'`-^-' `-' `-'`---'`---'`-'`-' Version v" + Constants.VERSION);
            Console.WriteLine(@"  ___________________________________________________________________________   ");

            try
            {
                Core.Updatizer.Instance.OnStatusChanged += onStatusChanged;
                Core.Updatizer.Instance.OnCurrenciesUpdated += onCurrenciesUpdated;
                Core.Updatizer.Instance.OnWalletsUpdated += onWalletsUpdated;

                Core.Updatizer.Instance.Initialize();
                Core.Updatizer.Instance.Start();
            }
            catch(Exception e)
            {
                Logger.Instance.Error(e.Message);
            }

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static void clearLine(int left, int top)
        {
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(left, top);
        }

        private static void turn()
        {
            Task.Run(() =>
            {
                while (status == UpdatizerStatusEnum.UPDATING)
                {
                    counter++;

                    switch (counter % 4)
                    {
                        case 0: Console.Write("/"); counter = 0; break;
                        case 1: Console.Write("-"); break;
                        case 2: Console.Write("\\"); break;
                        case 3: Console.Write("|"); break;
                    }

                    Thread.Sleep(100);
                    try { Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop); } catch { };
                }
            });
        }

        #region Updatizer Events

        private static void onStatusChanged(UpdatizerStatusEnum updatizerStatusEnum)
        {
            status = updatizerStatusEnum;

            clearLine(0, 6);
            string statusText = string.Format("Status : {0}", status);
            Logger.Instance.Debug(statusText);

            Console.SetCursorPosition(statusText.Length + 9, 6);
            Thread.Sleep(100);

            turn();
        }

        private static void onCurrenciesUpdated(RealCurrenciesEnum realCurrency, double targetCurrencyRate, double lastBTCPrice, List<CurrencyEntry> currencies)
        {
            int lineIndex = 1;
            int currentLine = 0;

            string[] currenciesInformations = new string[(currencies.Count + CURRENCIES_PER_LINE - 1) / CURRENCIES_PER_LINE];

            for (int i = 0; i < currencies.Count; i++)
            {
                if (lineIndex % CURRENCIES_PER_LINE == 0)
                {
                    currenciesInformations[currentLine] = currenciesInformations[currentLine] + string.Format("{0} : {1:N2} {2}.", currencies[i].Name,
                       (currencies[i].Name != Constants.BTC_CURRENCY_HEADER ? (currencies[i].LastEstimate * lastBTCPrice) : currencies[i].LastEstimate) * targetCurrencyRate, realCurrency.ToString());

                    currentLine += 1;
                    lineIndex = 1;
                }
                else
                {
                    if ((i + 1) == currencies.Count)
                    {
                        currenciesInformations[currentLine] = currenciesInformations[currentLine] + string.Format("{0} : {1:N2} {2}.", currencies[i].Name,
                        (currencies[i].Name != Constants.BTC_CURRENCY_HEADER ? (currencies[i].LastEstimate * lastBTCPrice) : currencies[i].LastEstimate) * targetCurrencyRate, realCurrency.ToString());
                    }
                    else
                    {
                        currenciesInformations[currentLine] = currenciesInformations[currentLine] + string.Format("{0} : {1:N2} {2} - ", currencies[i].Name,
                        (currencies[i].Name != Constants.BTC_CURRENCY_HEADER ? (currencies[i].LastEstimate * lastBTCPrice) : currencies[i].LastEstimate) * targetCurrencyRate, realCurrency.ToString());
                    }

                    lineIndex++;
                }
            }

            currenciesInformationsLinesCount = currenciesInformations.Length;

            for (int i = 0; i < currenciesInformations.Length; i++)
            {
                clearLine(0, 8 + i);
                Logger.Instance.Info(currenciesInformations[i]);
            }
        }

        private static void onWalletsUpdated(RealCurrenciesEnum realCurrency, List<WalletEntry> wallets)
        {
            for (int i = 0; i < wallets.Count; i++)
            {
                clearLine(0, 9  + currenciesInformationsLinesCount + i);

                var wallet = wallets[i];

                StringBuilder walletInformationsBuilder = new StringBuilder();
                walletInformationsBuilder.AppendFormat("{0} : {1:N2} {2}.", wallet.Label, wallet.LastRealEstimate, realCurrency);
                
                if(wallet.MissingCurrencies)
                {
                    var missingCurrencies = wallet.Holdings.Where(c => c.LastEstimate == -1).ToList();
                    if(missingCurrencies.Count() > 0)
                    {
                        walletInformationsBuilder.AppendFormat(" (Without ");
                        for (int j = 0; j < missingCurrencies.Count(); j++)
                        {
                            if((j + 1) != missingCurrencies.Count())
                            {
                                walletInformationsBuilder.AppendFormat("{0}, ", missingCurrencies[j].Name);
                            }
                            else
                            {
                                walletInformationsBuilder.AppendFormat("{0})", missingCurrencies[j].Name);
                            }
                        }
                    }
                }

                Logger.Instance.Info(walletInformationsBuilder.ToString());
            }
        }

        #endregion
    }
}
