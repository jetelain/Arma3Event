using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class SubscriptionInitialViewModel
    {
        public int? MatchSideID { get; set; }
        public int? RoundSquadID { get; set; }
        public User User { get; set; }

        [Display(Name = "Opération")]
        public Match Match { get; set; }

        [Display(Name = "J'accepte le traitement des données nécessaires aux inscriptions")]
        [Required]
        public bool AcceptSubscription { get; set; }

        [Display(Name = "J'ai lu et j'accepte le règlement de l'opération")]
        [Required]
        public bool AcceptMatchRules { get; set; }
    }
}
