using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Arma3GameInfos;
using ArmaEvent.Arma3GameInfos;

namespace Arma3Event.Entities
{
    public class Faction
    {
        public int FactionID { get; set; }

        [Display(Name = "Drapeau")]
        public string Flag { get; set; }

        [Display(Name = "Marqueur sur carte")]
        public GameMarkerType? GameMarker { get; set; }

        [Display(Name = "Nom")]
        public string Name { get; set; }

        [Display(Name = "Coté usuel")]
        public GameSide UsualSide { get; set; }
    }
}
