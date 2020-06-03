using Microsoft.AspNetCore.Mvc.Rendering;
using Arma3Event.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Models
{
    public class RoundSquadFormViewModel
    {
        public RoundSquad Squad { get; set; }
        public List<SelectListItem> MatchUserDropdownList { get; internal set; }
        public List<SelectListItem> SquadLeadRoles { get; internal set; }
        public List<SelectListItem> SquadMemberRoles { get; internal set; }
    }
}
