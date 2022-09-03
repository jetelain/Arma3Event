using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BIS.Core.Config;

namespace Arma3ServerToolbox.ArmaPersist
{
    public class PersistBackup
    {
        public PersistBackup()
        {
        }

        public PersistBackup (string name, DateTime dt, string server)
        {
            this.Name = name;
            this.LastChange = dt;
            this.Server = server;
        }

        public List<PersistPlayer> Players { get; set; } = new List<PersistPlayer>();
        public List<PersistBox> Boxes { get; set; } = new List<PersistBox>();
        public List<PersistVehicle> Vehicles { get; set; } = new List<PersistVehicle>();

        public string Name { get; set; }
        public DateTime LastChange { get; set; }
        public string Server { get; set; }

        public static List<PersistBackup> Read(Stream stream, DateTime dt, string server)
        {
            var file = new ParamFile(stream);
            var profile = file.Root.Entries.OfType<ParamClass>().FirstOrDefault(c => c.Name == "ProfileVariables");
            var backupEntries = profile.Entries.OfType<ParamClass>().Where(e => ((e.Entries.OfType<ParamValue>().FirstOrDefault(e => e.Name == "name").Value.Value as string) ?? "").StartsWith("gtd_persistence:")).ToList();
            var backups = new List<PersistBackup>();
            foreach (var backupEntry in backupEntries)
            {
                var backupData = (List<object>)DeserializeContent(backupEntry.Entries.OfType<ParamClass>().FirstOrDefault(e => e.Name == "data"));
                var backupName = (string)backupEntry.Entries.OfType<ParamValue>().FirstOrDefault(e => e.Name == "name").Value.Value;
                var backup = new PersistBackup(backupName.Substring(16), dt, server);
                if (backupData[0] != null)
                {
                    foreach (List<object> playerData in (List<object>)backupData[0])
                    {
                        backup.Players.Add(new PersistPlayer(playerData));
                    }
                }
                int id = 0;
                if (backupData[1] != null)
                {
                    foreach (List<object> boxData in (List<object>)backupData[1])
                    {
                        id++;
                        if (boxData != null)
                        {
                            backup.Boxes.Add(new PersistBox(boxData, id));
                        }
                    }
                }
                id = 0;
                if (backupData[2] != null)
                {
                    foreach (List<object> vehicleData in (List<object>)backupData[2])
                    {
                        id++;
                        if (vehicleData != null)
                        {
                            backup.Vehicles.Add(new PersistVehicle(vehicleData, id));
                        }
                    }
                }
                backups.Add(backup);
            }
            return backups;
        }


        private static object DeserializeContent(ParamClass data)
        {
            if (data.Name != "data")
            {
                throw new InvalidOperationException($"Expected 'data' but is '{data.Name}'");
            }

            var type = (ParamValue)data.Entries[0];
            if (type.Name == "nil")
            {
                return null;
            }
            if (type.Name != "singleType")
            {
                throw new InvalidOperationException($"Expected 'singleType' but is '{type.Name}'");
            }

            if (data.Entries.Count == 1)
            {
                switch (type.Value.Value as string)
                {
                    case "ARRAY":
                        return new List<object>();
                    case "NOTHING":
                    default:
                        return null;
                }
            }

            var value = data.Entries[1];
            if (value.Name != "value")
            {
                throw new InvalidOperationException($"Expected 'value' but is '{value.Name}'");
            }

            switch (type.Value.Value as string)
            {
                case "ARRAY":
                    return DeserializeArray((ParamClass)value);
                case "STRING":
                    return  (string)((ParamValue)value).Value.Value;
                case "SCALAR":
                    return (float)((ParamValue)value).Value.Value;
                case "BOOL":
                    return (int)((ParamValue)value).Value.Value == 0 ? false : true;
                case "NOTHING":
                default:
                    return null;
            }
        }

        private static List<object> DeserializeArray(ParamClass value)
        {
            if (value.Name != "value")
            {
                throw new InvalidOperationException($"Expected 'value' but is '{value.Name}'");
            }
            var result = new List<object>();
            foreach (var entry in value.Entries.OfType<ParamClass>())
            {
                var itemName = "Item" + result.Count;
                if (entry.Name != itemName)
                {
                    throw new InvalidOperationException($"Expected '{itemName}' but is '{entry.Name}'");
                }
                result.Add(DeserializeContent((ParamClass)entry.Entries[0]));
            }
            return result;
        }

    }
}
