using System;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Models;
using Arma3Event.Services.ArmaPersist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminReviewPersistController : Controller
    {
        private readonly Arma3EventContext _context;
        private readonly IConfiguration _config;
        private readonly PersistService _persist;

        public AdminReviewPersistController(Arma3EventContext context, IConfiguration config, PersistService persist)
        {
            _context = context;
            _config = config;
            _persist = persist;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminReviewPersistViewModel();
            vm.Backups = _persist.GetBackups();
            vm.Matchs = await _context.Matchs.Where(m => m.StartDate >= DateTime.Today).Take(1).ToListAsync();
            return View(vm);
        }

        public async Task<IActionResult> Match(int id)
        {
            var match = await GetMatch(id);
            if (match == null)
            {
                return NotFound();
            }
            var backup = _persist.GetBackups().FirstOrDefault();
            if (backup == null)
            {
                return NotFound();
            }
            var vm = new AdminReviewPersistDetailsViewModel();
            vm.Backup = backup;
            vm.Match = match;
            return View("Details", vm);
        }

        public async Task<IActionResult> Details(string server, string backupName, int id)
        {
            var match = await GetMatch(id);
            if (match == null)
            {
                return NotFound();
            }
            var backup = _persist.GetBackup(server, backupName);
            if (backup == null)
            {
                return NotFound();
            }
            var vm = new AdminReviewPersistDetailsViewModel();
            vm.Backup = backup;
            vm.Match = match;
            return View(vm);
        }

        private async Task<Match> GetMatch(int id)
        {
            return await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.GameMap)
                .Include(m => m.MatchTechnicalInfos)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads).ThenInclude(s => s.Slots).ThenInclude(s => s.AssignedUser).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Faction)
                .FirstOrDefaultAsync(m => m.MatchID == id);
        }
    }
}