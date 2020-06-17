using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;
using Arma3Event.Models;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminMatchUsersController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminMatchUsersController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: AdminMatchUsers/Edit/5
        public async Task<IActionResult> Create(int matchId)
        {
            var match = await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads).ThenInclude(s => s.Slots)
                .Include(m => m.GameMap)
                .Include(m => m.Users)
                .FirstOrDefaultAsync(m => m.MatchID == matchId);
            if (match == null)
            {
                return NotFound();
            }

            var vm = new MatchUserCreateViewModel();
            vm.MatchUser = new MatchUser() 
            { 
                MatchID = match.MatchID,
                Match = match
            };
            if (match.Sides.Count ==1)
            {
                vm.MatchUser.Side = match.Sides.First();
                vm.MatchUser.MatchSideID = vm.MatchUser.Side.MatchSideID;
            }
            vm.Users = (await _context.Users.ToListAsync()).Where(u => !match.Users.Any(mu => mu.UserID == u.UserID)).Select(u => new SelectListItem(u.Name,u.UserID.ToString())).ToList();
            PrepareEditViewModel(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatchUserCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vm.MatchUser);

                await _context.SaveChangesAsync();

                using (var transac = await _context.Database.BeginTransactionAsync())
                {
                    await ApplyUserSlots(vm);

                    await _context.SaveChangesAsync();

                    await transac.CommitAsync();
                }

                return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = vm.MatchUser.MatchID }, "users");
            }
            PrepareEditViewModel(vm);
            return View(vm);
        }


        // GET: AdminMatchUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers
                .Include(m => m.Match).ThenInclude(r => r.GameMap)
                .Include(m => m.Side)
                .Include(m => m.User)
                .Include(m => m.Slots).ThenInclude(s => s.Squad).ThenInclude(r => r.Side).ThenInclude(s => s.Round)
                .FirstOrDefaultAsync(m => m.MatchUserID == id);
            if (matchUser == null)
            {
                return NotFound();
            }

            return View(matchUser);
        }


        // GET: AdminMatchUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers
                .Include(m => m.Match).ThenInclude(r => r.GameMap)
                .Include(m => m.Match).ThenInclude(s => s.Rounds)
                .Include(m => m.Match).ThenInclude(s => s.Rounds).ThenInclude(r => r.Sides).ThenInclude(r => r.Squads).ThenInclude(s => s.Slots)
                .Include(m => m.Side)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MatchUserID == id);
            if (matchUser == null)
            {
                return NotFound();
            }
            var vm = new MatchUserEditViewModel();
            vm.MatchUser = matchUser;
            PrepareEditViewModel(vm);
            return View(vm);
        }

        private void PrepareEditViewModel(MatchUserEditViewModel vm)
        {
            vm.MatchSideDropdownList = new SelectList(_context.MatchSides.Where(m => m.MatchID == vm.MatchUser.MatchID), "MatchSideID", "Name", vm.MatchUser.MatchSideID);
            vm.SlotPerRound = vm.MatchUser.Match.Rounds.OrderBy(r => r.Number).Select(r => CreateVM(r, vm.MatchUser)).ToList();
        }

        private UserRoundSlotViewModel CreateVM(Round r, MatchUser matchUser)
        {
            return new UserRoundSlotViewModel()
            {
                Round = r,
                RoundSlotID = r.Sides.SelectMany(s => s.Squads).SelectMany(s => s.Slots).FirstOrDefault(s => s.MatchUserID == matchUser.MatchUserID)?.RoundSlotID
            };
        }

        // POST: AdminMatchUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MatchUserEditViewModel vm)
        {
            if (id != vm.MatchUser.MatchUserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var transac = await _context.Database.BeginTransactionAsync())
                    {
                        _context.Update(vm.MatchUser);

                        await ApplyUserSlots(vm);

                        await _context.SaveChangesAsync();

                        await transac.CommitAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchUserExists(vm.MatchUser.MatchUserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = vm.MatchUser.MatchID }, "users");
            }
            PrepareEditViewModel(vm); 
            return View(vm);
        }

        private async Task ApplyUserSlots(MatchUserEditViewModel vm)
        {
            if (vm.MatchUser.MatchSideID == null)
            {
                var previousSlots = await _context.RoundSlots.Where(s => s.MatchUserID == vm.MatchUser.MatchUserID).ToListAsync();
                foreach (var slot in previousSlots)
                {
                    slot.MatchUserID = null;
                    slot.IsValidated = false;
                    slot.SetTimestamp();
                    _context.Update(slot);
                }
            }
            else
            {
                var slotIds = vm.SlotPerRound.Where(s => s.RoundSlotID != null).Select(s => s.RoundSlotID).ToList();

                var previousSlots = await _context.RoundSlots.Where(s => !slotIds.Contains(s.RoundSlotID) && s.MatchUserID == vm.MatchUser.MatchUserID).ToListAsync();
                foreach (var slot in previousSlots)
                {
                    slot.MatchUserID = null;
                    slot.IsValidated = false;
                    slot.SetTimestamp();
                    _context.Update(slot);
                }

                var newSlots = await _context.RoundSlots.Where(s => slotIds.Contains(s.RoundSlotID) && s.Squad.Side.MatchSide.MatchSideID == vm.MatchUser.MatchSideID).ToListAsync();
                foreach (var slot in newSlots)
                {
                    slot.MatchUserID = vm.MatchUser.MatchUserID;
                    slot.IsValidated = true;
                    slot.SetTimestamp();
                    _context.Update(slot);
                }
            }
        }

        // GET: AdminMatchUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers
                .Include(m => m.Match)
                .Include(m => m.Side)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MatchUserID == id);
            if (matchUser == null)
            {
                return NotFound();
            }

            return View(matchUser);
        }

        // POST: AdminMatchUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var matchUser = await _context.MatchUsers.FindAsync(id);
            _context.MatchUsers.Remove(matchUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = matchUser.MatchID }, "users");
        }

        private bool MatchUserExists(int id)
        {
            return _context.MatchUsers.Any(e => e.MatchUserID == id);
        }
    }
}
