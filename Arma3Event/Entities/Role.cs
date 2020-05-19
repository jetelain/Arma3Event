using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public enum Role
    {
        [Display(Name = "Chef de groupe")]
        SquadLeader,

        [Display(Name = "Chef d'équipe")]
        TeamLeader,

        [Display(Name = "Soldat")]
        Member
    }
}