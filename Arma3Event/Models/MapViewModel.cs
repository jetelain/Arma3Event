using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class MapViewModel
    {
        public GameMap GameMap { get { return Match.GameMap; } }

        public Match Match { get; set; }
        public RoundSide RoundSide { get; internal set; }
        public Round Round { get; internal set; }
    }
}
