using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public enum MatchState
    {
        [Display(Name = "inscription ouverte")]
        Open = 0,
        
        [Display(Name = "inscription ouverte, mise en ligne des ordres pour la mission")]
        OpenOrdersReady = 1,
        
        [Display(Name = "inscription fermée, mission brief avant mission")]
        ClosedBrief = 2,
        
        [Display(Name = "opération en cours")]
        InProgress = 3,
        
        [Display(Name = "opération terminée")]
        Over = 4
    }
}