using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Hubs;
using Arma3Event.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        [HttpGet]
        public async Task<IActionResult> MapExport(int id)
        {
            return Content(await MapExportJson(id));
        }

        [HttpGet]
        public async Task<IActionResult> MapExportSqf(int id)
        {
            var script = @"private _data = " + await MapExportJson(id) + @";

_data params ['_icons', '_rects', '_metis'];

if (!isNil 'gtd_map_allMarkers') then {
  {
    deleteMarker _x;
  } forEach gtd_map_allMarkers;
};

if (!isNil 'gtd_map_allMetisMarkers') then {
  {
    [_x] call mts_markers_fnc_deleteMarker
  } forEach gtd_map_allMetisMarkers;
};

gtd_map_allMarkers = [];
gtd_map_allMetisMarkers = [];

{
  _x params ['_id', '_x', '_y', '_w', '_h', '_color', '_rotate'];
  private _marker = createMarker [ format ['_USER_DEFINED #0/planops%1/0', _id], [_x, _y]];
  _marker setMarkerShape 'RECTANGLE';
  _marker setMarkerBrush 'SolidBorder';
  _marker setMarkerDir _rotate;
  _marker setMarkerColor _color; 
  _marker setMarkerSize [_w,2];
  gtd_map_allMarkers pushBack _marker;
} forEach _rects;

{
  _x params ['_id', '_x', '_y', '_icon', '_color', '_text', '_rotate'];
  private _marker = createMarker [ format ['_USER_DEFINED #0/planops%1/0', _id], [_x, _y]];
  _marker setMarkerShape 'ICON';
  _marker setMarkerDir _rotate;
  _marker setMarkerColor _color; 
  _marker setMarkerText _text;
  _marker setMarkerType _icon;
  gtd_map_allMarkers pushBack _marker;
} forEach _icons;

{
  _x params ['_id', '_x', '_y', '_sideid', '_dashed', '_icon', '_mod1', '_mod2', '_size', '_designation'];
  private _marker = [[_x,_y], 0, true, [_sideid, _dashed], [_icon, _mod1, _mod2], [_size, false, false], [], _designation] call mts_markers_fnc_createMarker;
  gtd_map_allMetisMarkers pushBack _marker;
} forEach _metis;

publicVariable 'gtd_map_allMarkers';
publicVariable 'gtd_map_allMetisMarkers';";
            return Content(script);
        }

        private async Task<string> MapExportJson(int id)
        {
            var markers = await _context.MapMarkers.Where(m => m.MatchID == id).ToListAsync();
            var iconMarkers = new List<List<object>>();
            var rectMarkers = new List<List<object>>();
            var metisMarkers = new List<List<object>>();
            foreach (var marker in markers)
            {
                var data = JsonConvert.DeserializeObject<MarkerData>(marker.MarkerData);
                if (data.type == "basic")
                {
                    var dir = Get(data.config, "dir", "");

                    iconMarkers.Add(new List<object>() {
                        marker.MapMarkerID,
                        data.pos[1],
                        data.pos[0],
                        data.symbol,
                        Get(data.config, "color", "ColorBlack"),
                        Get(data.config, "label", ""),
                        !string.IsNullOrEmpty(dir) ? (double.Parse(dir) * 360d / 6400d) : 0d });
                }
                else if (data.type == "line")
                {
                    for (int i = 2; i < data.pos.Length; i += 2)
                    {
                        var y1 = data.pos[i - 2];
                        var x1 = data.pos[i - 1];
                        var y2 = data.pos[i];
                        var x2 = data.pos[i + 1];

                        var length = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

                        rectMarkers.Add(new List<object>() {
                            marker.MapMarkerID,
                            x1 + ((x2 - x1) / 2d),
                            y1 + ((y2 - y1) / 2d),
                            length / 2d,
                            2,
                            Get(data.config, "color", "ColorBlack"),
                            GetAngle((x2 - x1),(y2 - y1)) * -1 });
                    }
                }
                else if (data.type == "mil")
                {
                    metisMarkers.Add(new List<object>()
                    {
                        marker.MapMarkerID,
                        data.pos[1],
                        data.pos[0],
                        ToIdentify(data.symbol[3]),
                        ToDashed(data.symbol[3], data.symbol[6]),
                        ToIcon(data.symbol.Substring(10, 6)),
                        ToMod1(data.symbol.Substring(16, 2)),
                        ToMod2(data.symbol.Substring(18, 2)),
                        ToSize(data.symbol.Substring(8, 2)),
                        Get(data.config, "uniqueDesignation", null) ?? Get(data.config, "higherFormation", null) ?? ""
                    });
                }
            }
            return JsonConvert.SerializeObject(new[] { iconMarkers, rectMarkers, metisMarkers });
        }

        private int ToSize(string v)
        {
            switch(v)
            {
                case "11": return 1;
                case "12": return 2;
                case "13": return 3;
                case "14": return 4;
                case "15": return 5;
                case "16": return 6;
                case "17": return 7;
                case "18": return 8;
                case "21": return 9;
                case "22": return 10;
                case "23": return 11;
                case "24": return 12;
            }
            return 0;
        }

        private int ToMod2(string v)
        {
            return 0;
        }

        private int ToMod1(string v)
        {
            switch(v)
            {
                case "98": return 7;
            }
            return 0;
        }

        private int ToIcon(string v)
        {
            switch(v)
            {
                case "121100": return 1; // Infantry
                case "121102": return 2; // Mechanized Infantry
                case "121103": return 1; // Infantry with Main Gun System
                case "121104": return 3; // Motorized Infantry
                case "121105": return 2; // Mechanized Infantry with Main Gun System
                case "120500": return 4; // Armor
                case "120600": return 12; // Rotary Wing
                case "121000": return 37; // Combined Arms
                case "150600": return 0; // Intercept
            }
            return 0;
        }

        private bool ToDashed(char i, char v) 
        {
            return v == '1' || i == '5' || i == '2';
        }

        private string ToIdentify(char v)
        {
            switch(v)
            {
                case '2':
                case '3':
                    return "blu";
                case '4':
                    return "neu";
                case '5':
                case '6':
                    return "red";
            }
            return "unk";
        }

        private double GetAngle(double dx, double dy)
        {
            return Math.Atan2(dy, dx) * 180d / Math.PI;
        }

        private string Get(Dictionary<string, string> config, string key, string defaultValue)
        {
            string value;
            if (config.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
