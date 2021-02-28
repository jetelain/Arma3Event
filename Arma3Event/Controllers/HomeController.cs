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
using Arma3TacMapLibrary.TacMaps;

namespace Arma3Event.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Arma3EventContext _context;
        private readonly IApiTacMaps _tacMaps;

        public HomeController(ILogger<HomeController> logger, Arma3EventContext context, IApiTacMaps tacMaps)
        {
            _logger = logger;
            _context = context;
            _tacMaps = tacMaps;
        }
        private async Task<User> GetUser()
        {
            return await UserHelper.GetUser(_context, User);
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();
            vm.User = await GetUser();
            var maxDate = DateTime.Today;
            if (vm.User != null)
            {
                vm.Matchs = await _context.Matchs.Where(m => m.StartDate >= maxDate).OrderBy(m => m.StartDate).Include(m => m.MatchTechnicalInfos).Include(m => m.Rounds).Include(m => m.Users).ToListAsync();
            }
            else
            {
                vm.Matchs = await _context.Matchs.Where(m => m.StartDate >= maxDate).OrderBy(m => m.StartDate).Include(m => m.Rounds).ToListAsync();
            }
            vm.News = await _context.News.OrderByDescending(n => n.Date).FirstOrDefaultAsync();
            vm.Videos = await _context.Videos.OrderByDescending(n => n.Date).Take(3).ToListAsync();


            vm.NextMatch = vm.Matchs.FirstOrDefault();
            if (vm.NextMatch != null && vm.NextMatch.TacMapId != null)
            {
                vm.NextMatch.TacMap = await _tacMaps.Get(vm.NextMatch.TacMapId.Value);
            }
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            return View(await _context.ContentBlocks.Where(b => b.Kind == ContentBlockKind.AboutPage).OrderBy(n => n.OrderNum).ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
