using Updatizer.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updatizer.Core.Managers
{
    public static class StorageManager
    {
        public static HoldingsFile HoldingsFile = null;

        public static void Initiliaze()
        {
            if (!File.Exists(Constants.HOLDINGS_FILE_NAME))
                throw new Exception("Unable to find your holdings file !");

            try
            {
                HoldingsFile = readJsonFile<HoldingsFile>(Constants.HOLDINGS_FILE_NAME);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to read your holdings file : {0}.", e.Message));
            }
        }

        private static T readJsonFile<T>(string path)
        {
            var reader = new StreamReader(path);
            var content = reader.ReadToEnd();
            reader.Close();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
