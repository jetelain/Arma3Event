using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Arma3GameInfos
{
    public static class GameMarkerTypeHelper
    {
        public static IEnumerable<GameMarkerType> GetAll()
        {
            return Enum.GetValues(typeof(GameMarkerType)).Cast<GameMarkerType>();
        }
    }
}
