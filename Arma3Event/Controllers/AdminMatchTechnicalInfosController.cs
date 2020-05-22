using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Threading;
using System.Xml.XPath;
using System.Text;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminMatchTechnicalInfosController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminMatchTechnicalInfosController(Arma3EventContext context)
        {
            _context = context;
        }


        // GET: AdminMatchTechnicalInfos/Create
        public async Task<IActionResult> Create(int matchID)
        {
            var matchTechnicalInfos = new MatchTechnicalInfos()
            {
                MatchID = matchID,
                GameServerPort = 2302,
                VoipServerPort = 9987
            };
            await LoadInformations(matchTechnicalInfos);
            return View(matchTechnicalInfos);
        }

        private async Task LoadInformations(MatchTechnicalInfos matchTechnicalInfos)
        {
            matchTechnicalInfos.Match = await _context.Matchs.FindAsync(matchTechnicalInfos.MatchID);
        }

        // POST: AdminMatchTechnicalInfos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatchTechnicalInfos matchTechnicalInfos, IFormFile modpack)
        {
            if (ModelState.IsValid)
            {
                await ProcessModpack(matchTechnicalInfos, modpack);
                _context.Add(matchTechnicalInfos);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AdminMatchsController.Details), ControllersName.AdminMatchs, new { id = matchTechnicalInfos.MatchID });
            }
            await LoadInformations(matchTechnicalInfos);
            return View(matchTechnicalInfos);
        }

        // GET: AdminMatchTechnicalInfos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchTechnicalInfos = await _context.MatchTechnicalInfos.Include(m => m.Match).FirstOrDefaultAsync(t => t.MatchTechnicalInfosID == id);
            if (matchTechnicalInfos == null)
            {
                return NotFound();
            }
            return View(matchTechnicalInfos);
        }

        // POST: AdminMatchTechnicalInfos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MatchTechnicalInfos matchTechnicalInfos, IFormFile modpack)
        {
            if (id != matchTechnicalInfos.MatchTechnicalInfosID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!await ProcessModpack(matchTechnicalInfos, modpack))
                    {
                        var existing = await _context.MatchTechnicalInfos.AsNoTracking().FirstAsync(i => i.MatchTechnicalInfosID == matchTechnicalInfos.MatchTechnicalInfosID);
                        matchTechnicalInfos.ModsCount = existing.ModsCount;
                        matchTechnicalInfos.ModsDefinition = existing.ModsDefinition;
                        matchTechnicalInfos.ModsLastChange = existing.ModsLastChange;
                    }

                    _context.Update(matchTechnicalInfos);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchTechnicalInfosExists(matchTechnicalInfos.MatchTechnicalInfosID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AdminMatchsController.Details), ControllersName.AdminMatchs, new { id = matchTechnicalInfos.MatchID  } );
            }
            await LoadInformations(matchTechnicalInfos);
            return View(matchTechnicalInfos);
        }

        private async Task<bool> ProcessModpack(MatchTechnicalInfos matchTechnicalInfos, IFormFile modpack)
        {
            if (modpack != null)
            {
                using(var reader = new StreamReader(modpack.OpenReadStream(), Encoding.UTF8))
                {
                    matchTechnicalInfos.ModsDefinition = await reader.ReadToEndAsync();
                }
                matchTechnicalInfos.ModsLastChange = DateTime.Now;
                var doc = XDocument.Parse(matchTechnicalInfos.ModsDefinition);
                matchTechnicalInfos.ModsCount = doc.Descendants("tr").Attributes("data-type").Where(a => a.Value == "ModContainer").Count();
                return true;
            }
            return false;
        }

        private bool MatchTechnicalInfosExists(int id)
        {
            return _context.MatchTechnicalInfos.Any(e => e.MatchTechnicalInfosID == id);
        }
    }
}
