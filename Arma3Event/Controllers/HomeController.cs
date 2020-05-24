using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arma3Event.Entities;
using Arma3Event.Models;

namespace Arma3Event.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Arma3EventContext _context;

        public HomeController(ILogger<HomeController> logger, Arma3EventContext context)
        {
            _logger = logger;
            _context = context;
        }
        private async Task<User> GetUser()
        {
            var steamId = SteamHelper.GetSteamId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);
            return user;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();
            vm.User = await GetUser();
            var maxDate = DateTime.Today;
            if (vm.User != null)
            {
                vm.Matchs = await _context.Matchs.Where(m => m.StartDate >= maxDate).OrderBy(m => m.StartDate).Include(m => m.Rounds).Include(m => m.Users).ToListAsync();
            }
            else
            {
                vm.Matchs = await _context.Matchs.Where(m => m.StartDate >= maxDate).OrderBy(m => m.StartDate).Include(m => m.Rounds).ToListAsync();
            }
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
