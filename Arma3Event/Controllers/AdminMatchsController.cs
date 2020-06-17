using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class AdminMatchsController : Controller
    {
        private readonly Arma3EventContext _context;

        public AdminMatchsController(Arma3EventContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index()
        {
            return View(await _context.Matchs.OrderBy(m => m.StartDate).ToListAsync());
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var match = await _context.Matchs
                .Include(r => r.GameMap)
                .Include(m => m.Sides)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Faction)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .Include(m => m.Users).ThenInclude(u => u.Slots)
                .Include(m => m.MatchTechnicalInfos)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }
            SortModel(match);

            ViewBag.DuplicableRoundSides = await _context.RoundSides
                .Include(r => r.MatchSide).ThenInclude(s => s.Match)
                .Where(s => s.MatchSide.SquadsPolicy != SquadsPolicy.Unrestricted && s.MatchSide.MatchID != id)
                .OrderBy(s => s.MatchSide.Match.StartDate)
                .ToListAsync();

            return View(match);
        }

        public async Task<IActionResult> DuplicateFromOther(int roundSideID, int otherRoundSideID)
        {
            var roundSide = await _context.RoundSides
                .Include(rs => rs.Round)
                .Include(rs => rs.Squads).ThenInclude(s => s.Slots)
                .FirstOrDefaultAsync(s => s.RoundSideID == roundSideID);
            if (roundSide == null)
            {
                return NotFound();
            }
            var round = roundSide.Round;

            var otherRoundSide = await _context.RoundSides
                .Include(rs => rs.Round)
                .Include(rs => rs.Squads).ThenInclude(s => s.Slots)
                .FirstOrDefaultAsync(s => s.RoundSideID == otherRoundSideID);
            if (roundSide == null)
            {
                return NotFound();
            }
            await DuplicateSquadsAndSlots(otherRoundSide, roundSide, false);

            return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = round.MatchID }, "round-" + round.RoundID);
        }

        public async Task<IActionResult> DuplicateFromPrevious(int roundSideID)
        {
            var roundSide = await _context.RoundSides
                .Include(rs => rs.Round)
                .Include(rs => rs.Squads).ThenInclude(s => s.Slots)
                .FirstOrDefaultAsync(s => s.RoundSideID == roundSideID);
            if (roundSide == null)
            {
                return NotFound();
            }
            var round = roundSide.Round;

            var previousRound= await _context.Rounds.FirstOrDefaultAsync(r => r.MatchID == round.MatchID && r.Number == round.Number - 1);
            if (previousRound == null)
            {
                return NotFound();
            }

            var previousRoundSide = await _context.RoundSides
                .Include(rs => rs.Round)
                .Include(rs => rs.Squads).ThenInclude(s => s.Slots)
                .FirstOrDefaultAsync(s => s.RoundID == previousRound.RoundID && s.MatchSideID == roundSide.MatchSideID);
            if (previousRoundSide == null)
            {
                return NotFound();
            }

            await DuplicateSquadsAndSlots(previousRoundSide, roundSide, true);

            return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = round.MatchID }, "round-" + round.RoundID);
        }

        private async Task DuplicateSquadsAndSlots(RoundSide source, RoundSide target, bool includeUser)
        {
            _context.RemoveRange(target.Squads);

            target.Squads = source.Squads.Select(s => DuplicateSquadAndSlots(target, s, includeUser)).ToList();

            _context.AddRange(target.Squads);

            await _context.SaveChangesAsync();
        }

        private static RoundSquad DuplicateSquadAndSlots(RoundSide target, RoundSquad s, bool includeUser)
        {
            var copy = new RoundSquad()
            {
                InviteOnly = s.InviteOnly,
                Name = s.Name,
                UniqueDesignation = s.UniqueDesignation,
                RestrictTeamComposition = s.RestrictTeamComposition,
                SlotsCount = s.SlotsCount,
                RoundSideID = target.RoundSideID,
            };
            copy.Slots = s.Slots.Select(p => new RoundSlot()
            {
                Label = p.Label,
                MatchUserID = includeUser ? p.MatchUserID : null,
                IsValidated = includeUser ? p.IsValidated : false,
                Role = p.Role,
                SlotNumber = p.SlotNumber,
                Squad = copy,
                Timestamp = p.Timestamp
            }).ToList();
            return copy;
        }


        // GET: Matches/Create
        public async Task<IActionResult> Create()
        {
            var vm = new MatchFormViewModel();

            vm.Match = new Match()
            {
                Date = DateTime.Today,
                StartTime = new DateTime(1,1,1,21,0,0),
                Sides = new List<MatchSide>(),
                Template = MatchTemplate.SingleSideCooperative,
                Rounds = new List<Round>(),
                RulesLink = "/Events/Rules"
            };

            ApplyTemplate(vm);

            await PrepareDrowdownLists(vm);

            return View(vm);
        }

        private async Task PrepareDrowdownLists(MatchFormViewModel vm)
        {
            vm.MapsDropdownList = (await _context.Maps.OrderBy(m => m.Name).ToListAsync())
                .Select(m => new SelectListItem(m.Name, m.GameMapID.ToString())).ToList();
            vm.FactionsDropdownList = (await _context.Factions.OrderBy(m => m.Name).ToListAsync())
                .Select(m => new SelectListItem(m.Name, m.FactionID.ToString())).ToList();
            vm.FactionsData = await _context.Factions.ToListAsync(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetUserSide(int matchUserId, [FromForm]int matchSideId)
        {
            var matchUser = await _context.MatchUsers.FindAsync(matchUserId);

            matchUser.MatchSideID = matchSideId;

            _context.Update(matchUser);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = matchUser.MatchID }, "unassigned");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSlots(int id, int matchUserId)
        {
            var slots = await _context.RoundSlots.Where(s => s.MatchUserID == matchUserId && s.AssignedUser.MatchID == id).ToListAsync();
            foreach(var slot in slots)
            {
                slot.IsValidated = true;
                _context.Update(slot);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), ControllersName.AdminMatchs, new { id = id }, "users");
        }

        // POST: Matches/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatchFormViewModel vm, string applyTemplate)
        {
            ApplyTemplate(vm);

            if (ModelState.IsValid && string.IsNullOrEmpty(applyTemplate))
            {
                _context.Add(vm.Match);
                foreach (var side in vm.Match.Sides)
                {
                    _context.Add(side);
                }
                foreach (var round in vm.Match.Rounds)
                {
                    _context.Add(round);
                }
                foreach (var roundSide in vm.Match.Rounds.SelectMany(r => r.Sides))
                {
                    _context.Add(roundSide);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await PrepareDrowdownLists(vm);

            return View(vm);
        }
        private void ApplyTemplate(MatchFormViewModel vm)
        {
            int wantedRounds;
            int wantedSides;

            switch (vm.Match.Template)
            {
                default:
                case MatchTemplate.SingleSideCooperative:
                    wantedRounds = 1;
                    wantedSides = 1;
                    break;
                case MatchTemplate.TwoRoundsTwoSidesCompetitive:
                    wantedRounds = 2;
                    wantedSides = 2;
                    break;
                case MatchTemplate.TwoRoundsThreeSidesCompetitive:
                    wantedRounds = 2;
                    wantedSides = 3;
                    break;
            }

            if (vm.Match.Rounds.Count > wantedRounds)
            {
                vm.Match.Rounds.RemoveRange(wantedRounds, vm.Match.Rounds.Count - wantedRounds);
            }
            else if (vm.Match.Rounds.Count < wantedRounds)
            {
                vm.Match.Rounds.AddRange(Enumerable.Range(vm.Match.Rounds.Count, wantedRounds - vm.Match.Rounds.Count).Select(i => new Round() { Number = i + 1, Sides = new List<RoundSide>() }));
            }

            if (vm.Match.Sides.Count > wantedSides)
            {
                vm.Match.Sides.RemoveRange(wantedSides, vm.Match.Sides.Count - wantedSides);
                foreach(var round in vm.Match.Rounds)
                {
                    round.Sides.RemoveRange(wantedSides, round.Sides.Count - wantedSides);
                }
            }
            else if (vm.Match.Sides.Count < wantedSides)
            {
                vm.Match.Sides.AddRange(Enumerable.Range(vm.Match.Sides.Count, wantedSides - vm.Match.Sides.Count).Select(i => new MatchSide() { Name = "Equipe " + ViewHelper.SideName(i), Number = i + 1, MaxUsersCount = 20 }));
                foreach (var round in vm.Match.Rounds)
                {
                    round.Sides.AddRange(Enumerable.Range(round.Sides.Count, wantedSides - round.Sides.Count).Select(i => new RoundSide()));
                }
            }

            ConsolidateMatchForm(vm);
        }

        private void ConsolidateMatchForm(MatchFormViewModel vm)
        {
            for (var s = 0; s < vm.Match.Sides.Count; ++s)
            {
                var side = vm.Match.Sides[s];
                side.Match = vm.Match;
                side.Number = s + 1;
            }
            
            for (var r = 0; r < vm.Match.Rounds.Count; ++r)
            {
                var round = vm.Match.Rounds[r];
                round.Number = r + 1;
                round.Match = vm.Match;
                for (var s = 0; s < round.Sides.Count; ++s)
                {
                    var roundSide = round.Sides[s];
                    roundSide.Round = round;
                    roundSide.MatchSide = vm.Match.Sides[s];
                    //roundSide.GameSide = GetSide(r, s);
                }
            }
        }


        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.Rounds)
                .ThenInclude(r => r.Sides)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }
            SortModel(match);
            var vm = new MatchFormViewModel();
            vm.Match = match;
            await PrepareDrowdownLists(vm);
            return View(vm);
        }

        internal static void SortModel(Match match)
        {
            match.Sides = match.Sides.OrderBy(s => s.Number).ToList();
            match.Rounds = match.Rounds.OrderBy(s => s.Number).ToList();
            foreach (var r in match.Rounds)
            {
                r.Sides = r.Sides.OrderBy(s => s.MatchSide.Number).ToList();
                foreach (var s in r.Sides)
                {
                    if (s.Squads != null)
                    {
                        s.Squads = s.Squads.OrderBy(a => a.UniqueDesignation).ToList();
                    }
                }
            }

            foreach (var s in match.Sides)
            {
                if (s.Users != null)
                {
                    s.Users = s.Users.OrderBy(s => s.User.Name).ToList();
                }
            }
            if (match.Users != null)
            {
                match.Users = match.Users.OrderBy(s => s.User.Name).ToList();
            }
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MatchFormViewModel vm, string applyTemplate)
        {
            if (id != vm.Match.MatchID)
            {
                return NotFound();
            }

            ApplyTemplate(vm);

            if (ModelState.IsValid && string.IsNullOrEmpty(applyTemplate))
            {
                ConsolidateMatchForm(vm);

                try
                {
                    _context.Update(vm.Match);
                    foreach (var side in vm.Match.Sides)
                    {
                        _context.Update(side);

                        if (side.SquadsPolicy == SquadsPolicy.SquadsAndSlotsRestricted)
                        {
                            var squadsToRestrict = await _context.RoundSquads.Where(s => s.Side.MatchSideID == side.MatchSideID && !s.RestrictTeamComposition).ToListAsync();
                            foreach(var squad in squadsToRestrict)
                            {
                                squad.RestrictTeamComposition = true;
                                _context.Update(squad);
                            }
                        }
                    }
                    foreach (var round in vm.Match.Rounds)
                    {
                        if (round.RoundID == 0)
                        {
                            _context.Add(round);
                        }
                        else
                        {
                            _context.Update(round);
                        }
                    }
                    foreach (var roundSide in vm.Match.Rounds.SelectMany(r => r.Sides))
                    {
                        if (roundSide.RoundSideID == 0)
                        {
                            _context.Add(roundSide);
                        }
                        else
                        {
                            _context.Update(roundSide);
                        }
                    }

                    var remains = vm.Match.Rounds.Where(r => r.RoundID != 0).Select(r => r.RoundID).ToList();

                    foreach (var removed in _context.Rounds.Where(r => r.MatchID == vm.Match.MatchID && !remains.Contains(r.RoundID)).ToList())
                    {
                        _context.Remove(removed);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(vm.Match.MatchID))
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
            await PrepareDrowdownLists(vm);

            return View(vm);
        }



        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matchs
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matchs.FindAsync(id);
            _context.Matchs.Remove(match);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatchExists(int id)
        {
            return _context.Matchs.Any(e => e.MatchID == id);
        }

        [HttpGet]
        public async Task<IActionResult> PreviewModPack(int id)
        {
            var match = await _context.Matchs.Include(m => m.MatchTechnicalInfos).FirstOrDefaultAsync(m => m.MatchID == id);
            if (string.IsNullOrEmpty(match?.MatchTechnicalInfos?.ModsDefinition))
            {
                return NotFound();
            }
            return File(Encoding.UTF8.GetBytes(match.MatchTechnicalInfos.ModsDefinition), "text/html; charset=utf-8");
        }
    }
}
