using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;
using Arma3Event.Models;

namespace Arma3Event.Controllers
{
    public class NewsController : Controller
    {
        private readonly Arma3EventContext _context;

        public NewsController(Arma3EventContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? matchID)
        {
            if (matchID != null)
            {
                return View(await _context.News.Where(n => n.MatchID == matchID).Include(n => n.Match).ThenInclude(m => m.GameMap).OrderByDescending(n => n.Date).ToListAsync());
            }
            return View(await _context.News.Include(n => n.Match).ThenInclude(m => m.GameMap).OrderByDescending(n => n.Date).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .Include(n => n.Match).ThenInclude(m => m.GameMap)
                .FirstOrDefaultAsync(m => m.NewsID == id);
            if (news == null)
            {
                return NotFound();
            }

            var vm = new NewsDetailsViewModel();
            vm.News = news;
            vm.Next = await _context.News.OrderBy(n => n.Date).Where(n => n.Date > news.Date).FirstOrDefaultAsync();
            vm.Previous = await _context.News.OrderByDescending(n => n.Date).Where(n => n.Date < news.Date).FirstOrDefaultAsync();
            return View(vm);
        }
    }
}
