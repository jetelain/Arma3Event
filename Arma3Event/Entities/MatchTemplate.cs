using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public enum MatchTemplate
    {
        [Display(Name = "Mission coopérative avec un seul camp/une seule armée")]
        SingleSideCooperative,

        [Display(Name = "Mission compétitive en deux manches, avec deux camps")]
        TwoRoundsTwoSidesCompetitive,

        [Display(Name = "Mission compétitive en deux manches, avec trois camps")]
        TwoRoundsThreeSidesCompetitive
    }
}
