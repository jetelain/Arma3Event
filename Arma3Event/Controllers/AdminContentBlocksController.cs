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
    public class AdminContentBlocksController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminContentBlocksController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: AdminContentBlocks
        public async Task<IActionResult> Index()
        {
            return View(await _context.ContentBlocks.ToListAsync());
        }

        // GET: AdminContentBlocks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentBlock = await _context.ContentBlocks
                .FirstOrDefaultAsync(m => m.ContentBlockID == id);
            if (contentBlock == null)
            {
                return NotFound();
            }

            return View(contentBlock);
        }

        // GET: AdminContentBlocks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminContentBlocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContentBlockID,Kind,Title,OrderNum,Content")] ContentBlock contentBlock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contentBlock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contentBlock);
        }

        // GET: AdminContentBlocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentBlock = await _context.ContentBlocks.FindAsync(id);
            if (contentBlock == null)
            {
                return NotFound();
            }
            return View(contentBlock);
        }

        // POST: AdminContentBlocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContentBlockID,Kind,Title,OrderNum,Content")] ContentBlock contentBlock)
        {
            if (id != contentBlock.ContentBlockID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contentBlock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContentBlockExists(contentBlock.ContentBlockID))
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
            return View(contentBlock);
        }

        // GET: AdminContentBlocks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentBlock = await _context.ContentBlocks
                .FirstOrDefaultAsync(m => m.ContentBlockID == id);
            if (contentBlock == null)
            {
                return NotFound();
            }

            return View(contentBlock);
        }

        // POST: AdminContentBlocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contentBlock = await _context.ContentBlocks.FindAsync(id);
            _context.ContentBlocks.Remove(contentBlock);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContentBlockExists(int id)
        {
            return _context.ContentBlocks.Any(e => e.ContentBlockID == id);
        }
    }
}
