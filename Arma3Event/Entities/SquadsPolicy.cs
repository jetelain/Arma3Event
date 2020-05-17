using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public enum SquadsPolicy
    {
        [Display(Name = "Aucune restriction")]
        Unrestricted,

        [Display(Name = "Définition des groupes par les organisateurs")]
        SquadsRestricted,

        [Display(Name = "Définition des groupes et de leur composition par les organisateurs")]
        SquadsAndSlotsRestricted
    }
}