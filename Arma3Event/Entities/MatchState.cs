using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public enum MatchState
    {
        [Display(Name = "inscription ouverte")]
        Open = 0,
        
        [Display(Name = "préparation de la mission par les chef de groupe, inscription fermée")]
        ClosedWarmup = 1,
        
        [Display(Name = "vérification des véhicules et du matériel avant depart, inscription fermée")]
        ClosedCheckup = 2,
        
        [Display(Name = "opération en cours")]
        InProgress = 3,
        
        [Display(Name = "opération terminée")]
        Over = 4
    }
}