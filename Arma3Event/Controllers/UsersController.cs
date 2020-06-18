using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Authorization;
using Arma3Event.Models;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "LoggedUser")]
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
            var user = await UserHelper.GetUser(_context, User);
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

            var self = await UserHelper.GetUser(_context, User);

            if (self.UserID == user.UserID)
            {
                ViewBag.IsSelf = true;
                ViewBag.UsePassword = await _context.UserLogins.AnyAsync(l => l.UserID == self.UserID);
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit()
        {
            var user = await UserHelper.GetUser(_context, User);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Password()
        {
            var user = await UserHelper.GetUser(_context, User);
            if (user == null)
            {
                return NotFound();
            }
            var userPwd = await _context.UserLogins.FirstOrDefaultAsync(u => u.UserID == user.UserID);
            return View(new PasswordViewModel()
            {
                Login = userPwd?.Login,
                NeedOldPassword = userPwd != null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password([Bind("Login,OldPassword,Password,PasswordRepeat")] PasswordViewModel vm)
        {
            var user = await UserHelper.GetUser(_context, User);
            if (user == null)
            {
                return NotFound();
            }
            var userPwd = await _context.UserLogins.FirstOrDefaultAsync(u => u.UserID == user.UserID);
            if (vm.Password != vm.PasswordRepeat)
            {
                ModelState.AddModelError("PasswordRepeat", "Les deux mots de passe ne correspondent pas.");
            }
            else if (await _context.UserLogins.AnyAsync(u => u.UserID != user.UserID && u.Login.ToLower() == vm.Login.ToLower()))
            {
                ModelState.AddModelError("Login", "Le nom d'utilisateur est déjà utilisé.");
            }
            else if (userPwd != null && !userPwd.IsValidPassword(vm.OldPassword))
            {
                ModelState.AddModelError("OldPassword", "L'ancien mot de passe ne corresponds pas.");
            }
            else if (ModelState.IsValid)
            {
                if (userPwd != null)
                {
                    userPwd.Login = vm.Login;
                    userPwd.SetPassword(vm.Password);
                    _context.Update(userPwd);
                }
                else
                {
                    userPwd = new UserLogin();
                    userPwd.UserID = user.UserID;
                    userPwd.Login = vm.Login;
                    userPwd.SetPassword(vm.Password);
                    _context.Add(userPwd);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            vm.OldPassword = string.Empty;
            vm.Password = string.Empty;
            vm.PasswordRepeat = string.Empty;
            vm.NeedOldPassword = userPwd != null;
            return View(vm);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,PrivacyOptions")] User userData)
        {
            var user = await UserHelper.GetUser(_context, User);

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
            var user = await UserHelper.GetUser(_context, User);

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
            var user = await UserHelper.GetUser(_context, User);
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
