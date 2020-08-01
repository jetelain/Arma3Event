using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public class Document
    {
        public int DocumentID { get; set; }

        [Display(Name = "Type")]
        public DocumentType Type { get; set; }

        [Display(Name = "Lien")]
        public string Link { get; set; }

        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Opération")]
        public int? MatchID { get; set; }
        [Display(Name = "Opération")]
        public Match Match { get; set; }
    }
}
