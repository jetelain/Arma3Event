using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Arma3Event.Entities
{
    public class RoundSquad
    {
        internal static string[] UniqueDesignations = new[] { "10", "11", "12", "13", "14", "20", "21", "22", "23", "24", "30", "31", "32", "33", "34", "40", "41", "42", "43", "44" };

        public int RoundSquadID { get; set; }

        [Display(Name = "Identifiant unique")]
        public string UniqueDesignation { get; set; }

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
        public string GenericName { get { return $"Groupe {UniqueDesignation}"; } }
    }


}
