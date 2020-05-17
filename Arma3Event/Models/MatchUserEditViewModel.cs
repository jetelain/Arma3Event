using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class MatchUserEditViewModel
    {
        public MatchUser MatchUser { get; set; }
        public SelectList MatchSideDropdownList { get; internal set; }
        public List<UserRoundSlotViewModel> SlotPerRound { get; set; }
    }
}
