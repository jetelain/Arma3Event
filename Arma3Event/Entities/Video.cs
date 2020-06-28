using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public class Video
    {
        public int VideoID { get; set; }

        [Display(Name = "Lien (Youtube)")]
        public string VideoLink { get; set; }

        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Display(Name = "Image")]
        public string Image { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Opération")]
        public int? MatchID { get; set; }
        [Display(Name = "Opération")]
        public Match Match { get; set; }
    }
}
