using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public class News
    {
        public int NewsID { get; set; }

        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Display(Name = "Contenu")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Dernière mise à jour")]
        public DateTime LastUpdate { get; set; }

        [Display(Name = "Opération")]
        public int? MatchID { get; set; }
        [Display(Name = "Opération")]
        public Match Match { get; set; }
    }
}
