using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class HomeViewModel
    {
        public List<Match> Matchs { get; internal set; }
        public User User { get; internal set; }
        public News News { get; internal set; }
    }
}
