using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arma3Event.Models
{
    public class MatchUserCreateViewModel : MatchUserEditViewModel
    {
        public List<SelectListItem> Users { get; internal set; }
    }
}
