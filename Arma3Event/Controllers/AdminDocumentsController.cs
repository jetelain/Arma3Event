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
    public class AdminDocumentsController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminDocumentsController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: AdminDocuments
        public async Task<IActionResult> Index()
        {
            var arma3EventContext = _context.Documents.Include(d => d.Match);
            return View(await arma3EventContext.ToListAsync());
        }

        // GET: AdminDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Match)
                .FirstOrDefaultAsync(m => m.DocumentID == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: AdminDocuments/Create
        public IActionResult Create()
        {
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name");
            return View();
        }

        // POST: AdminDocuments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocumentID,Type,Link,Title,Date,MatchID")] Document document)
        {
            if (ModelState.IsValid)
            {
                document.Date = DateTime.Now;
                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name", document.MatchID);
            return View(document);
        }

        // GET: AdminDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name", document.MatchID);
            return View(document);
        }

        // POST: AdminDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DocumentID,Type,Link,Title,Date,MatchID")] Document document)
        {
            if (id != document.DocumentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.DocumentID))
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
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name", document.MatchID);
            return View(document);
        }

        // GET: AdminDocuments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Match)
                .FirstOrDefaultAsync(m => m.DocumentID == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: AdminDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.DocumentID == id);
        }
    }
}
