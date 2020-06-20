using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public class ContentBlock
    {
        public int ContentBlockID { get; set; }

        [Display(Name = "Type de bloc")]
        public ContentBlockKind Kind { get; set; }

        [Display(Name = "Titre du bloc")]
        public string Title { get; set; }

        [Display(Name = "Numéro d'ordre")]
        public int OrderNum { get; set; }

        [Display(Name = "Contenu")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
    }
}
