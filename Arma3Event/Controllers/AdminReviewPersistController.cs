using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Models;
using Arma3Event.Services.ArmaPersist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminReviewPersistController : Controller
    {
        private readonly Arma3EventContext _context;
        private readonly IConfiguration _config;

        public AdminReviewPersistController(Arma3EventContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private static readonly string filePath = "/home/arma3-public/.local/share/Arma 3 - Other Profiles/server/server.vars.Arma3Profile";

        public async Task<IActionResult> Index()
        {
            var vm = new AdminReviewPersistViewModel();
            vm.Backups = new List<PersistBackup>();
            var servers = _config.GetSection("Servers");
            foreach(var entry in servers.GetChildren())
            {
                var hostName = entry.Key;
                var password = entry.Value;
                using (var client = new SftpClient(hostName, "arma3-public", password))
                {
                    client.Connect();
                    if ( client.Exists(filePath) )
                    {
                        var mem = new MemoryStream();
                        client.DownloadFile(filePath, mem);
                        mem.Position = 0;
                        vm.Backups.AddRange(PersistBackup.Read(mem, client.GetLastWriteTime(filePath), hostName));
                    }
                    client.Disconnect();
                }
            }
            vm.Matchs = await _context.Matchs.Where(m => m.StartDate >= DateTime.Today).Take(1).ToListAsync();
            return View(vm);
        }

        // /home/arma3-public/.local/share/Arma 3 - Other Profiles/server/server.vars.Arma3Profile

        public async Task<IActionResult> Details(string server, string backupName, int id)
        {
            var match = await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.GameMap)
                .Include(m => m.MatchTechnicalInfos)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads).ThenInclude(s => s.Slots).ThenInclude(s => s.AssignedUser).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Faction)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }

            var backup = GetBackup(server, backupName);
            if (backup == null)
            {
                return NotFound();
            }

            var vm = new AdminReviewPersistDetailsViewModel();

            vm.Backup = backup;
            vm.Match = match;

            return View(vm);
        }

        private PersistBackup GetBackup(string server, string backupName)
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