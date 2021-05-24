using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arma3ServerToolbox.ArmaPersist;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;

namespace Arma3Event.Services
{
    public class PersistService
    {
        private readonly IConfiguration _config;

        public PersistService(IConfiguration config)
        {
            _config = config;
        }

        private static readonly string filePath = "/home/arma3/.steam/steamcmd/server/profiles/Users/server/server.vars.Arma3Profile";

        public List<PersistBackup> GetBackups()
        {
            var servers = _config.GetSection("Servers");
            var backups = new List<PersistBackup>();
            foreach (var entry in servers.GetChildren())
            {
                var hostName = entry.Key;
                var password = entry.Value;
                using (var client = new SftpClient(hostName, "gamemanager", password))
                {
                    client.Connect();
                    if (client.Exists(filePath))
                    {
                        var mem = new MemoryStream();
                        client.DownloadFile(filePath, mem);
                        mem.Position = 0;
                        backups.AddRange(PersistBackup.Read(mem, client.GetLastWriteTime(filePath), hostName));
                    }
                    client.Disconnect();
                }
            }
            return backups.OrderByDescending(b => b.LastChange).ToList();
        }

        public PersistBackup GetBackup(string server, string backupName)
        {
            var servers = _config.GetSection("Servers");
            var entry = servers.GetSection(server);
            if (entry == null)
            {
                return null;
            }
            PersistBackup backup = null;
            var hostName = entry.Key;
            var password = entry.Value;
            using (var client = new SftpClient(hostName, "arma3-public", password))
            {
                client.Connect();
                if (client.Exists(filePath))
                {
                    var mem = new MemoryStream();
                    client.DownloadFile(filePath, mem);
                    mem.Position = 0;
                    backup = PersistBackup.Read(mem, client.GetLastWriteTime(filePath), hostName).FirstOrDefault(b => b.Name == backupName);
                }
                client.Disconnect();
            }
            return backup;
        }

    }
}
