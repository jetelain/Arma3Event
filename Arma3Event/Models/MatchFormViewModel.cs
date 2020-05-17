using System.Collections.Generic;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arma3Event.Models
{
    public class MatchFormViewModel
    {
        public Match Match { get; set; }
        public List<SelectListItem> MapsDropdownList { get; internal set; }
        public List<SelectListItem> FactionsDropdownList { get; internal set; }
        public List<Faction> FactionsData { get; internal set; }
    }
}
