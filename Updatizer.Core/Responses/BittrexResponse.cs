using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updatizer.Core.Requests
{
    public class BittrexResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
    }
}
