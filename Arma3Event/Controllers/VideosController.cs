using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;

namespace Arma3Event.Controllers
{
    public class VideosController : Controller
    {
        private readonly Arma3EventContext _context;

        public VideosController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: Videos
        public async Task<IActionResult> Index()
        {
            var arma3EventContext = _context.Videos.Include(v => v.Match).OrderByDescending(v => v.Date);
            return View(await arma3EventContext.ToListAsync());
        }
    }
}
