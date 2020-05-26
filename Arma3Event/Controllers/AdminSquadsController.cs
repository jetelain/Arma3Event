using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminSquadsController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminSquadsController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: RoundSquads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roundSquad = await _context.RoundSquads
                .Include(r => r.Side).ThenInclude(r => r.MatchSide).ThenInclude(r => r.Match).ThenInclude(r => r.GameMap)
                .Include(r => r.Side).ThenInclude(r => r.Round).ThenInclude(r => r.Match)
                .Include(r => r.Side).ThenInclude(r => r.Faction)
                .Include(r => r.Slots).ThenInclude(s => s.AssignedUser).ThenInclude(u => u.User)
                .FirstOrDefaultAsync(m => m.RoundSquadID == id);
            if (roundSquad == null)
            {
                return NotFound();
            }

            EnsurePolicy(roundSquad);

            return View(roundSquad);
        }

        // GET: RoundSquads/Create
        public async Task<IActionResult> Create(int roundSideID)
        {
            var vm = new RoundSquadFormViewModel();
            vm.Squad = new RoundSquad()
            {
                RoundSideID = roundSideID,
                Slots = Enumerable.Range(1, Constants.BaselineSquadMembers).Select(num => new RoundSlot() { SlotNumber = num }).ToList()
            };
            vm.Squad.Slots[0].Role = Role.SquadLeader;

            await PrepareViewModel(vm);
            await PrepareDrowndownList(vm);
            await ComputeSquadNumber(vm.Squad);
            return View(vm);
        }

        private static void EnsurePolicy(RoundSquad roundSquad)
        {
            if (roundSquad.Side.MatchSide.SquadsPolicy == SquadsPolicy.SquadsAndSlotsRestricted)
            {
                roundSquad.RestrictTeamComposition = true;
            }
        }

        private async Task PrepareViewModel(RoundSquadFormViewModel vm)
        {
            vm.Squad.Side = await _context.RoundSides
                .Include(r => r.Round).ThenInclude(r => r.Match).ThenInclude(r => r.GameMap)
                .Include(r => r.MatchSide).ThenInclude(r => r.Match)
                .FirstOrDefaultAsync(r => r.RoundSideID == vm.Squad.RoundSideID);

            EnsurePolicy(vm.Squad);
        }

        // POST: RoundSquads/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoundSquadFormViewModel vm)
        {
            await PrepareViewModel(vm);

            if (ModelState.IsValid)
            {
                for (int i = 0; i < vm.Squad.Slots.Count; ++i)
                {
                    var slot = vm.Squad.Slots[i];
                    slot.Squad = vm.Squad;
                }
                using (var transac = await _context.Database.BeginTransactionAsync())
                {
                    if (await CheckUserAvailibilty(vm))
                    {
                        await PrepareDrowndownList(vm);
                        return View(vm);
                    }

                    await ComputeSquadNumber(vm.Squad);

                    vm.Squad.Slots = vm.Squad.Slots.Where(s => s.Role != null).ToList();
                    vm.Squad.SlotsCount = vm.Squad.Slots.Count();

                    NormalizeSlotsNumber(vm);

                    _context.Add(vm.Squad);

                    foreach (var slot in vm.Squad.Slots)
                    {
                        _context.Add(slot);
                    }

                    await _context.SaveChangesAsync();
                    await transac.CommitAsync();
                }
                return RedirectToRound(vm.Squad);
            }
            await PrepareDrowndownList(vm);
            return View(vm);
        }

        private async Task ComputeSquadNumber(RoundSquad squad)
        {
            if (string.IsNullOrEmpty(squad.UniqueDesignation))
            {
                var others = await _context.RoundSquads.Where(rs => rs.RoundSideID == squad.RoundSideID).ToListAsync();
                if (others.Count == 0)
                {
                    squad.UniqueDesignation = RoundSquad.UniqueDesignations.First();
                }
                else
                {
                    squad.UniqueDesignation = RoundSquad.UniqueDesignations.First(num => !others.Any(t => t.UniqueDesignation == num));
                }
            }
            if (string.IsNullOrEmpty(squad.Name))
            {
                squad.Name = squad.GenericName;
            }
        }

        // GET: RoundSquads/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var roundSquad = await _context.RoundSquads
                .Include(r => r.Side).ThenInclude(r => r.MatchSide).ThenInclude(r => r.Match).ThenInclude(r => r.GameMap)
                .Include(r => r.Side).ThenInclude(r => r.Round).ThenInclude(r => r.Match)
                .Include(r => r.Slots)
                .FirstOrDefaultAsync(r => r.RoundSquadID == id);
            if (roundSquad == null)
            {
                return NotFound();
            }
            var vm = new RoundSquadFormViewModel()
            {
                Squad = roundSquad
            };
            vm.Squad.Slots = vm.Squad.Slots.Concat(Enumerable.Range(vm.Squad.Slots.Count, Constants.BaselineSquadMembers - vm.Squad.Slots.Count).Select(num => new RoundSlot() { SlotNumber = num })).ToList();
            EnsurePolicy(vm.Squad);

            await PrepareDrowndownList(vm);

            return View(vm);
        }

        private async Task PrepareDrowndownList(RoundSquadFormViewModel vm)
        {
            var sideUsers = await _context.MatchUsers
                .Include(u => u.User)
                .Include(u => u.Slots).ThenInclude(s => s.Squad)
                .Where(u => u.MatchSideID == vm.Squad.Side.MatchSideID)
                .OrderBy(u => u.User.Name)
                .ToListAsync();

            vm.MatchUserDropdownList = sideUsers
                .Where(u => !u.Slots.Any(s => s.Squad != null && s.Squad.RoundSideID == vm.Squad.RoundSideID && s.RoundSquadID != vm.Squad.RoundSquadID))
                .Select(u => new SelectListItem(u.User.Name, u.MatchUserID.ToString()))
                .ToList();
        }

        // POST: RoundSquads/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoundSquadFormViewModel vm)
        {
            if (id != vm.Squad.RoundSquadID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var transac = await _context.Database.BeginTransactionAsync())
                    {
                        await PrepareViewModel(vm);

                        var removed = vm.Squad.Slots.Where(s => s.Role == null && s.RoundSlotID != 0).ToList();

                        // Il y a un risque de concurrence d'accès, on s'assure que si un utilisateur s'est affecté entre temps que ce n'est pas perdu
                        if (await DetectConcurrentUpdates(vm) || await CheckUserAvailibilty(vm))
                        {
                            await PrepareDrowndownList(vm);
                            return View(vm);
                        }

                        vm.Squad.Slots = vm.Squad.Slots.Where(s => s.Role != null).ToList();
                        vm.Squad.SlotsCount = vm.Squad.Slots.Count();
                        _context.Update(vm.Squad);

                        // Numérote les slots, et s'assure que les utilisateurs n'ont pas de doublons
                        NormalizeSlotsNumber(vm);

                        foreach (var slot in vm.Squad.Slots)
                        {
                            slot.SetTimestamp();
                            if (slot.RoundSlotID == 0)
                            {
                                _context.Add(slot);
                            }
                            else
                            {
                                _context.Update(slot);
                            }
                        }

                        foreach (var slot in removed)
                        {
                            _context.Remove(slot);
                        }

                        await _context.SaveChangesAsync();
                        await transac.CommitAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoundSquadExists(vm.Squad.RoundSquadID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToRound(vm.Squad);
            }
            await PrepareViewModel(vm);
            await PrepareDrowndownList(vm);
            return View(vm);
        }

        private void NormalizeSlotsNumber(RoundSquadFormViewModel vm)
        {
            var slotNumber = 1;
            foreach (var slot in vm.Squad.Slots)
            {
                slot.SlotNumber = slotNumber;
                if (slotNumber == 1)
                {
                    slot.Role = Role.SquadLeader;
                }
                slotNumber++;
            }
        }

        private async Task<bool> CheckUserAvailibilty(RoundSquadFormViewModel vm)
        {
            ModelState.Clear();
            var badusers = 0;
            var slotsIds = vm.Squad.Slots.Where(s => s.RoundSlotID != 0).Select(s => s.RoundSlotID).ToList();

            foreach (var slot in vm.Squad.Slots.Where(s => s.Role != null && s.MatchUserID != null))
            {
                var conflicts = await _context.RoundSlots.Where(s => !slotsIds.Contains(s.RoundSlotID) 
                    && s.MatchUserID == slot.MatchUserID 
                    && s.Squad.Side.RoundID == vm.Squad.Side.RoundID).CountAsync();
                if (conflicts != 0)
                {
                    var key = $"Squad.Slots[{slot.SlotNumber - 1}].MatchUserID";
                    ModelState.AddModelError(key, "Le participant n'est plus disponible.");
                    badusers++;
                }
            }
            return badusers != 0;
        }

        private async Task<bool> DetectConcurrentUpdates(RoundSquadFormViewModel vm)
        {
            ModelState.Clear();
            var concurrent = 0;
            foreach (var slot in vm.Squad.Slots.Where(s => s.Role != null && s.RoundSlotID != 0))
            {
                var existing = await _context.RoundSlots.AsNoTracking().FirstOrDefaultAsync(s => s.RoundSlotID == slot.RoundSlotID);
                if (slot.Timestamp < existing.Timestamp && existing.MatchUserID != slot.MatchUserID)
                {
                    var key = $"Squad.Slots[{slot.SlotNumber - 1}].MatchUserID";
                    slot.MatchUserID = existing.MatchUserID;
                    ModelState.AddModelError(key, "La valeur a été modifiée entre temps par un autre utilisateur.");
                    concurrent++;
                }
            }
            return concurrent != 0;
        }

        private IActionResult RedirectToRound(RoundSquad rs)
        {
            return RedirectToAction(nameof(AdminMatchsController.Details), ControllersName.AdminMatchs, new { id = rs.Side.Round.MatchID }, "round-" + rs.Side.RoundID);
        }

        // GET: RoundSquads/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roundSquad = await _context.RoundSquads
                .Include(r => r.Side).ThenInclude(r => r.MatchSide).ThenInclude(r => r.Match).ThenInclude(r => r.GameMap)
                .Include(r => r.Side).ThenInclude(r => r.Round).ThenInclude(r => r.Match)
                .Include(r => r.Side).ThenInclude(r => r.Faction)
                .FirstOrDefaultAsync(m => m.RoundSquadID == id);
            if (roundSquad == null)
            {
                return NotFound();
            }

            return View(roundSquad);
        }

        // POST: RoundSquads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roundSquad = await _context.RoundSquads
                .Include(r => r.Side).ThenInclude(r => r.Round)
                .FirstOrDefaultAsync(r => r.RoundSquadID == id);
            _context.RoundSquads.Remove(roundSquad);
            await _context.SaveChangesAsync();
            return RedirectToRound(roundSquad);
        }

        private bool RoundSquadExists(int id)
        {
            return _context.RoundSquads.Any(e => e.RoundSquadID == id);
        }
    }
}
