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
        public string WorkshopLink { get; set; }
        public string WebMap { get; set; }
    }
}
