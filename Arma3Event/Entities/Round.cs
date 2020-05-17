using System.Collections.Generic;

namespace Arma3Event.Entities
{
    public class Round
    {
        public int RoundID { get; set; }

        public int Number { get; set; }

        public int MatchID { get; set; }

        public Match Match { get; set; }

        public List<RoundSide> Sides { get; set; }

    }
}