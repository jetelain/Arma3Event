using System;
using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public class RoundSlot
    {
        public int RoundSlotID { get; set; }

        public int SlotNumber { get; set; }

        public int RoundSquadID { get; set; }

        public RoundSquad Squad { get; set; }

        [Display(Name = "Libellé")]
        public string Label { get; set; }

        [Display(Name = "Inscript")]
        public int? MatchUserID { get; set; }

        [Display(Name = "Inscript")]
        public MatchUser AssignedUser { get; set; }

        public long? Timestamp { get; set; }

        public Role? Role { get; set; }

        public bool IsValidated { get; set; }

        public void SetTimestamp()
        {
            Timestamp = DateTime.UtcNow.Ticks;
        }
    }
}