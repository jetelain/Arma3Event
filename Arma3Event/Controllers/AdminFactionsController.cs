using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminFactionsController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminFactionsController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: AdminFactions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Factions.OrderBy(f => f.Name).ToListAsync());
        }

        // GET: AdminFactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faction = await _context.Factions
                .FirstOrDefaultAsync(m => m.FactionID == id);
            if (faction == null)
            {
                return NotFound();
            }

            return View(faction);
        }

        // GET: AdminFactions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminFactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Faction faction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(faction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(faction);
        }

        // GET: AdminFactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faction = await _context.Factions.FindAsync(id);
            if (faction == null)
            {
                return NotFound();
            }
            return View(faction);
        }

        // POST: AdminFactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Faction faction)
        {
            if (id != faction.FactionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FactionExists(faction.FactionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(faction);
        }

        // GET: AdminFactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faction = await _context.Factions
                .FirstOrDefaultAsync(m => m.FactionID == id);
            if (faction == null)
            {
                return NotFound();
            }

            return View(faction);
        }

        // POST: AdminFactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var faction = await _context.Factions.FindAsync(id);
            _context.Factions.Remove(faction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FactionExists(int id)
        {
            return _context.Factions.Any(e => e.FactionID == id);
        }
    }
}
