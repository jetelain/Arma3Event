using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Hubs
{
    public class MapId
    {
        public MapId()
        {
        }

        public MapId(MapMarker marker)
        {
            matchID = marker.MatchID;
            roundSideID = marker.RoundSideID;
        }

        public int matchID { get; set; }
        public int? roundSideID { get; set; }

        public string GetGroup()
        {
            return roundSideID == null ? $"Match:{matchID}" : $"MatchRound:{roundSideID}";
        }
    }
}
