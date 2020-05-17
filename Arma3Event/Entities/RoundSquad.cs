using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Arma3Event.Entities
{
    public class RoundSquad
    {
        public int RoundSquadID { get; set; }

        public int Number { get; set; }

        [Display(Name = "Libellé")]
        public string Name { get; set; }

        public int RoundSideID { get; set; }

        [Display(Name = "Armée")]
        public RoundSide Side { get; set; }

        [NotMapped]
        public MatchUser Leader 
        { 
            get { return Slots?.FirstOrDefault(s => s.SlotNumber == 1)?.AssignedUser; } 
        }

        [Display(Name = "Permettre uniquement les emplacements prédéfinits")]
        public bool RestrictTeamComposition { get; set; }

        [Display(Name = "Accès sur invitation uniquement")]
        public bool InviteOnly { get; set; }

        public List<RoundSlot> Slots { get; set; }

        public int SlotsCount { get; set; }

        [NotMapped]
        public string GenericName { get { return $"Squad {Number}"; } }
    }


}
