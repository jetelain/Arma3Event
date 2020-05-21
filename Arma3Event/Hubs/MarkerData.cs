using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Hubs
{
    public class MarkerData
    {
        public string type { get; set; }
        public string symbol { get; set; }
        public Dictionary<string,string> config { get; set; }
        public double[] pos { get; set; }
    }
}
