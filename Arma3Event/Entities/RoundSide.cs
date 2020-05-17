using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ArmaEvent.Arma3GameInfos;

namespace Arma3Event.Entities
{
    public class RoundSide
    {
        public int RoundSideID { get; set; }

        public List<RoundSquad> Squads { get; set; }

        public int RoundID { get; set; }

        public Round Round { get; set; }

        public int MatchSideID { get; set; }

        public GameSide GameSide { get; set; }

        [Display(Name = "Armée")]
        public int? FactionID { get; set; }
        [Display(Name = "Armée")]
        public Faction Faction { get; set; }

        [Display(Name="Equipe")]
        public MatchSide MatchSide { get; set; }
    }
}
