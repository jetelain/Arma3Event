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
    [Authorize(Policy = "SteamID")]
    public class UsersController : Controller
    {
        private readonly Arma3EventContext _context;

        public UsersController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var steamId = SteamHelper.GetSteamId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), ControllersName.Home);
            }
            return RedirectToAction(nameof(Details), new { id = user.UserID });
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Matchs).ThenInclude(m => m.Match)
                .Include(u => u.Matchs).ThenInclude(m => m.Side)
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.IsSelf = SteamHelper.GetSteamId(User) == user.SteamId;

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit()
        {
            var steamId = SteamHelper.GetSteamId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,PrivacyOptions")] User userData)
        {
            var steamId = SteamHelper.GetSteamId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);

            if (ModelState.IsValid)
            {
                try
                {
                    user.Name = userData.Name;
                    user.PrivacyOptions = userData.PrivacyOptions;
                    user.SteamName = User.Identity.Name;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserID))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete()
        {
            var steamId = SteamHelper.GetSteamId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var steamId = SteamHelper.GetSteamId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(HomeController.Index), ControllersName.Home);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}
