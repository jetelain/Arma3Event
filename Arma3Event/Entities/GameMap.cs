using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public class GameMap
    {
        public int GameMapID { get; set; }

        [Display(Name = "Nom")]
        public string Name { get; set; }

        [Display(Name = "Image de fond")]
        public string Image { get; set; }

        [Display(Name = "Lien Workshop Steam")]
        public string WorkshopLink { get; set; }

        [Display(Name = "Identifiant carte web")]
        public string WebMap { get; set; }
    }
}
