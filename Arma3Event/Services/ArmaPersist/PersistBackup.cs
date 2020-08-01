﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenId.Steam;
using BIS.Core.Config;

namespace Arma3Event.Services.ArmaPersist
{
    public class PersistBackup
    {
        public PersistBackup (string name, DateTime dt, string server)
        {
            this.Name = name;
            this.LastChange = dt;
            this.Server = server;
        }

        public List<PersistPlayer> Players { get; } = new List<PersistPlayer>();
        public List<PersistVehicle> Vehicles { get; } = new List<PersistVehicle>();

        public string Name { get; }
        public DateTime LastChange { get; }
        public string Server { get; }

        public static List<PersistBackup> Read(Stream stream, DateTime dt, string server)
        {
            var file = new ParamFile(stream);
            var profile = file.Root.Entries.OfType<ParamClass>().FirstOrDefault(c => c.Name == "ProfileVariables");
            var backupEntries = profile.Entries.OfType<ParamClass>().Where(e => ((e.Entries.OfType<ParamValue>().FirstOrDefault(e => e.Name == "name").Value.Value as string) ?? "").StartsWith("gtd_persistence:")).ToList();
            var backups = new List<PersistBackup>();
            foreach (var backupEntry in backupEntries)
            {
                var backupData = ToArray(backupEntry.Entries.OfType<ParamClass>().FirstOrDefault(e => e.Name == "data"));
                var backupName = (string)backupEntry.Entries.OfType<ParamValue>().FirstOrDefault(e => e.Name == "name").Value.Value;
                var backup = new PersistBackup(backupName.Substring(16), dt, server);
                foreach (List<object> playerData in (List<object>)backupData[0])
                {
                    backup.Players.Add(new PersistPlayer(playerData));
                }
                int id = 0;
                foreach (List<object> vehicleData in (List<object>)backupData[2])
                {
                    id++;
                    if (vehicleData != null)
                    {
                        backup.Vehicles.Add(new PersistVehicle(vehicleData, id));
                    }
                }
                backups.Add(backup);
            }
            return backups;
        }

        private static List<object> ToArray(ParamClass paramClass)
        {
            var result = new List<object>();

            if (paramClass.Entries.Count == 1)
            {
                return result;
            }

            var content = (ParamClass)paramClass.Entries[1];
            foreach(var entry in content.Entries.OfType<ParamClass>())
            {
                var entryData = (ParamClass)entry.Entries[0];
                var type = ((ParamArray)((ParamClass)entryData.Entries[0]).Entries[0]).Array.Entries[0].Value as string;
                switch(type)
                {
                    case "ARRAY":
                        result.Add(ToArray(entryData));
                        break;
                    case "STRING":
                        result.Add((string)((ParamValue)entryData.Entries[1]).Value.Value);
                        break;
                    case "SCALAR":
                        result.Add((float)((ParamValue)entryData.Entries[1]).Value.Value);
                        break;
                    case "BOOL":
                        result.Add(((int)((ParamValue)entryData.Entries[1]).Value.Value) != 0);
                        break;
                    case "NOTHING":
                        result.Add(null);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
    }
}
