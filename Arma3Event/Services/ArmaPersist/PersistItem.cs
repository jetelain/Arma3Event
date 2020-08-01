using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Arma3Event.Services.ArmaPersist
{
    public class PersistItem
    {
        public PersistItem(string name, float count)
        {
            Name = name;
            Count = count;
        }

        public float Count { get; }

        public string Name { get; }

        internal static List<PersistItem> Load(List<object> list)
        {
            return list.Select(item => new PersistItem((string)((List<object>)item)[0], (float)((List<object>)item)[1])).ToList();
        }

        internal static List<PersistItem> LoadFromCargo(List<object> cargo)
        {
            var data = new List<PersistItem>();
            data.AddRange(LoadFromCargoEntry((List<object>)cargo[0]));
            data.AddRange(LoadFromCargoEntry((List<object>)cargo[1]));
            data.AddRange(LoadFromCargoEntry((List<object>)cargo[2]));
            data.AddRange(LoadFromCargoEntry((List<object>)cargo[3]));
            return data;
        }

        internal static List<PersistItem> LoadFromCargoEntry(List<object> cargoSpec)
        {
            var names = (List<object>)cargoSpec[0];
            var counts = (List<object>)cargoSpec[1];
            return names.Select((name, index) => new PersistItem((string)name, (float)counts[index]))
                .GroupBy(i => i.Name)
                .Select(g => new PersistItem(g.Key, g.Sum(i => i.Count)))
                .ToList();
        }
    }
}