using System.Collections;
using System.Collections.Generic;
using Arma3Event.Entities;
using Microsoft.EntityFrameworkCore.Query;

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

        public static string Icon(Role role)
        {
            if (role != null && !string.IsNullOrEmpty(role.Icon))
            {
                return role.Icon;
            }
            return "";
        }

        public static string Icon(Faction faction)
        {
            if (faction != null && !string.IsNullOrEmpty(faction.Icon))
            {
                return faction.Icon;
            }
            return "/img/factions/none.png";
        }

        public static string Style(GameMap map)
        {
            if (map != null && !string.IsNullOrEmpty(map.Image))
            {
                return $"background-image: url({map.Image});";
            }
            return "";
        }

    }
}
