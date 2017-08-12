using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updatizer.Core.Responses
{
    public class FixerLatestResponse
    {
        public string Base { get; set; }
        public string Date { get; set; }
        public dynamic Rates { get; set; }
    }
}
