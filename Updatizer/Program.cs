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
        static UpdatizerStatusEnum status;
        static int counter = 0;

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
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
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

            turn();
        }

        private static void onWalletsUpdated(RealCurrenciesEnum realCurrency, List<WalletEntry> wallets)
        {
            for (int i = 0; i < wallets.Count; i++)
            {
                clearLine(0, 7 + i);

                var wallet = wallets[i];

                StringBuilder walletInformationsBuilder = new StringBuilder();
                walletInformationsBuilder.AppendFormat("{0} : {1:N2} {2}", wallet.Label, wallet.LastRealEstimate, realCurrency);
                
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
