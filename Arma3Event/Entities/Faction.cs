using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaEvent.Arma3GameInfos;

namespace Arma3Event.Entities
{
    public class Faction
    {
        public int FactionID { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }

        public GameSide UsualSide { get; set; }
    }
}
