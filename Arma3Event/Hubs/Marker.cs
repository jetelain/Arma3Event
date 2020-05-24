using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Hubs
{
    public class Marker
    {
        public MapId mapId { get; set; }

        public int id { get; set; }

        public MarkerData data { get; set; }
    }
}
