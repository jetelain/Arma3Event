using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class UserRoundSlotViewModel
    {
        public int? RoundSlotID { get; set; }

        public Round Round { get; set; }
    }
}
