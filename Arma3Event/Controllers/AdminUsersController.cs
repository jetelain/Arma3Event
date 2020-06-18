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
using System.Text.RegularExpressions;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminUsersController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: AdminUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: AdminUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.ManualLogin)
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: AdminUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: AdminUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: AdminUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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

        // GET: AdminUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        //GeneratePassword

        public async Task<IActionResult> RemovePassword(int id)
        {
            var user = await _context.UserLogins
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.UserID == id && !string.IsNullOrEmpty(m.User.SteamId));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("RemovePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePasswordConfirmed([FromForm] int userLoginID)
        {
            var userLogin = await _context.UserLogins.FindAsync(userLoginID);
            _context.UserLogins.Remove(userLogin);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = userLogin.UserID });
        }

        public async Task<IActionResult> GeneratePassword(int id)
        {
            var user = await _context.Users
                .Include(u => u.ManualLogin)
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(new GeneratePasswordViewModel()
            {
                User = user,
                Login = user?.ManualLogin?.Login ?? GenerateLogin(user.Name),
                GeneratedPassword = UserHelper.GenerateRandomPassword()
            });
        }

        private static readonly Regex NonAlphaNum = new Regex("[^a-zA-Z0-9]+", RegexOptions.Compiled);

        private string GenerateLogin(string name)
        {
            return NonAlphaNum.Replace(name, "");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePassword(GeneratePasswordViewModel vm)
        {
            var user = await _context.Users
                .Include(u => u.ManualLogin)
                .FirstOrDefaultAsync(m => m.UserID == vm.User.UserID);

            if (user == null)
            {
                return NotFound();
            }

            if (await _context.UserLogins.AnyAsync(u => u.UserID != user.UserID && u.Login.ToLower() == vm.Login.ToLower()))
            {
                ModelState.AddModelError("Login", "Le nom d'utilisateur est déjà utilisé.");
            }
            else if (ModelState.IsValid)
            {
                if (user.ManualLogin != null)
                {
                    user.ManualLogin.Login = vm.Login;
                    user.ManualLogin.SetPassword(vm.GeneratedPassword);
                    _context.Update(user.ManualLogin);
                }
                else
                {
                    user.ManualLogin = new UserLogin();
                    user.ManualLogin.UserID = user.UserID;
                    user.ManualLogin.Login = vm.Login;
                    user.ManualLogin.SetPassword(vm.GeneratedPassword);
                    _context.Add(user.ManualLogin);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = user.UserID });
            }
            return View(new GeneratePasswordViewModel()
            {
                User = user,
                Login = vm.Login,
                GeneratedPassword = vm.GeneratedPassword
            });
        }

        

    }
}
