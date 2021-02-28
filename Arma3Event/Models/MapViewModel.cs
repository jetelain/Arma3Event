using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class MapViewModel
    {
        public Match Match { get; set; }

        public RoundSide RoundSide { get; set; }

        public Round Round { get; set; }

        public bool CanEditMap { get; set; }
        public string Hub { get; internal set; }
    }
}
