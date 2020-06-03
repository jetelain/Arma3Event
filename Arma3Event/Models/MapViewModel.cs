using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Hubs;

namespace Arma3Event.Models
{
    public class MapViewModel
    {
        public GameMap GameMap { get { return Match.GameMap; } }

        public Match Match { get; set; }

        public RoundSide RoundSide { get; set; }

        public Round Round { get; set; }

        public bool CanEditMap { get; set; }
        public MapId MapId { get; internal set; }
    }
}
