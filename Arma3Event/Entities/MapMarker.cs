using System;

namespace Arma3Event.Entities
{
    [Obsolete]
    public class MapMarker
    {
        public int MapMarkerID { get; set; }

        public int MatchID { get; set; }
        public Match Match { get; set; }

        public int? RoundSideID { get; set; }
        public RoundSide RoundSide { get; set; }

        public int? RoundSquadID { get; set; }
        public RoundSquad RoundSquad { get; set; }

        public int? MatchUserID { get; set; }
        public MatchUser MatchUser { get; set; }

        public string MarkerData { get; set; }
    }
}
