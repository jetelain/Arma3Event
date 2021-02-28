using System.Collections;
using Arma3Event.Entities;
using Arma3TacMapLibrary.Arma3;

namespace Arma3Event
{
    public static class ViewHelper
    {
        private static readonly string[] SideNames = new[] { "A", "B", "C", "D"};
        public static string SideName(MatchSide side)
        {
            return SideName(side.Number - 1);
        }

        public static string SideName(int sideIndex)
        {
            return SideNames[sideIndex];
        }
        public static string SideColClass(IList list)
        {
            return $"col-md-{12 / list.Count}";
        }

        public static string Icon(Role? role)
        {
            if (role != null)
            {
                return $"/img/roles/{role}.png";
            }
            return "";
        }

        public static string Icon(Faction faction)
        {
            if (faction != null && !string.IsNullOrEmpty(faction.Flag))
            {
                return faction.Flag;
            }
            return "/img/factions/none.png";
        }

        public static string Style(MapInfos map)
        {
            if (map != null && !string.IsNullOrEmpty(map.preview))
            {
                return $"background-image: url({map.preview});";
            }
            return "";
        }

    }
}
