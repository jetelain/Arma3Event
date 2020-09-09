using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arma3Event.Arma3GameInfos;
using Arma3Event.Entities;
using Arma3Event.Hubs;
using Arma3Event.Models;
using Arma3Event.Services.ArmaPersist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Arma3Event.Controllers
{
    public class EventsController : Controller
    {
        private readonly Arma3EventContext _context;
        private readonly IAuthorizationService _auth;
        private readonly PersistService _persist;

        public EventsController(Arma3EventContext context, IAuthorizationService auth, PersistService persist)
        {
            _context = context;
            _auth = auth;
            _persist = persist;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Matchs.Include(m => m.Rounds).ToListAsync());
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpGet]
        public async Task<IActionResult> ReviewEquipement(int id, int? roundSquadID)
        {
            var user = await GetUser();
            if (user == null)
            {
                return Forbid();
            }

            var match = await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.GameMap)
                .Include(m => m.MatchTechnicalInfos)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads).ThenInclude(s => s.Slots).ThenInclude(s => s.AssignedUser).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Faction)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }

            var matchUser = match.Users.FirstOrDefault(u => u.UserID == user.UserID);
            if (matchUser == null)
            {
                return Forbid();
            }

            var squads = match.Rounds.SelectMany(r => r.Sides).SelectMany(s => s.Squads);

            RoundSquad squad;
            if (roundSquadID == null)
            {
                squad = squads.First(s => s.Slots.Any(s => s.AssignedUser == matchUser && s.Role >= Role.SquadLeader));
                if ( squad == null )
                {
                    return Forbid();
                }
            }
            else
            {
                if (!(await _auth.AuthorizeAsync(User, "Admin")).Succeeded)
                {
                    return Forbid();
                }
                squad = match.Rounds.SelectMany(r => r.Sides).SelectMany(s => s.Squads).FirstOrDefault(s => s.RoundSquadID == roundSquadID);
                if (squad == null)
                {
                    return NotFound();
                }
            }

            var backup = _persist.GetBackups().FirstOrDefault();

            var vm = new ReviewEquipementViewModel();
            vm.Match = match;
            vm.Squad = squad;
            vm.Backup = backup;
            return View(vm);
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpGet]
        public async Task<IActionResult> Subscription(int? id, int? matchSideID, int? roundSquadID)
        {
            if (id == null)
            {
                return NotFound();
            }
            var match = await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.GameMap)
                .Include(m => m.MatchTechnicalInfos)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads).ThenInclude(s => s.Slots).ThenInclude(s => s.AssignedUser).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Faction)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }
            match.Documents = await _context.Documents.Where(v => v.MatchID == id).OrderBy(v => v.Date).ToListAsync();

            var user = await GetUser();
            if (user == null)
            {
                if (match.State != MatchState.Open)
                {
                    return RedirectToAction(nameof(Details), new { id });
                }
                return View("SubscriptionInitial", new SubscriptionInitialViewModel()
                {
                    Match = match,
                    MatchSideID = matchSideID,
                    RoundSquadID = roundSquadID,
                    User = new User() { SteamName = User.Identity.Name, Name = User.Identity.Name }
                });
            }
            var matchUser = match.Users.FirstOrDefault(u => u.UserID == user.UserID);
            if (matchUser == null)
            {
                if (match.State != MatchState.Open)
                {
                    return RedirectToAction(nameof(Details), new { id });
                }
                return View("SubscriptionInitial", new SubscriptionInitialViewModel()
                {
                    Match = match,
                    MatchSideID = matchSideID,
                    RoundSquadID = roundSquadID,
                    User = user
                });
            }
            var vm = new SubscriptionViewModel();
            vm.Match = match;
            vm.User = user;
            vm.MatchUser = matchUser;
            return View("Subscription", vm);
        }

        private async Task<User> GetUser()
        {
            return await UserHelper.GetUser(_context, User);
        }

        //
        [Authorize(Policy = "LoggedUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscriptionSide(int id, int matchSideID)
        {
            var user = await GetUser();
            if (user == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers.FirstOrDefaultAsync(u => u.MatchID == id && u.UserID == user.UserID);
            if (matchUser == null || matchUser.MatchSideID != null)
            {
                return NotFound();
            }

            matchUser.MatchSideID = await _context.MatchSides.Where(s => s.MatchSideID == matchSideID && s.MatchID == id).Select(s => s.MatchSideID).FirstOrDefaultAsync();
            if (matchUser.MatchSideID != null)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    if (await CanJoin(matchUser))
                    {
                        _context.Update(matchUser);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                }
            }
            return RedirectToAction(nameof(Subscription), new { id });
        }

        private async Task<bool> CanJoin(MatchUser user)
        {
            var max = await _context.MatchSides.Where(s => s.MatchSideID == user.MatchSideID).Select(s => s.MaxUsersCount).FirstOrDefaultAsync();
            var count = await _context.MatchUsers.Where(s => s.MatchSideID == user.MatchSideID && s.MatchUserID != user.MatchUserID).CountAsync();
            return count < max;
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscriptionInitial(int id, SubscriptionInitialViewModel vm)
        {
            var match = await _context.Matchs.Include(m => m.Sides).FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }
            if (match.State != MatchState.Open)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!vm.AcceptMatchRules)
            {
                ModelState.AddModelError("AcceptMatchRules", "Vous devez accepter le réglement de l'opération");
            }

            if (!vm.AcceptSubscription)
            {
                ModelState.AddModelError("AcceptSubscription", "Vous devez accepter le traitement des données nécessaires à votre inscription");
            }

            if (!ModelState.IsValid)
            {
                vm.Match = match;
                return View("SubscriptionInitial", vm);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var user = await GetUser();
                if (user == null)
                {
                    vm.User.SteamId = UserHelper.GetSteamId(User);
                    vm.User.SteamName = User.Identity.Name;
                    vm.User.UserID = 0;
                    _context.Add(vm.User);
                    await _context.SaveChangesAsync();
                    user = vm.User;
                }

                var matchUser = await _context.MatchUsers.FirstOrDefaultAsync(u => u.MatchID == id && u.UserID == user.UserID);
                if (matchUser == null)
                {
                    if (vm.RoundSquadID != null)
                    {
                        // Vérifie que RoundSquadID appartient bien à MatchID
                        vm.RoundSquadID = await _context.RoundSquads.Where(s => s.RoundSquadID == vm.RoundSquadID && s.Side.MatchSide.MatchID == id).Select(s => s.RoundSquadID).FirstOrDefaultAsync();
                        // Calcule MatchSideID
                        vm.MatchSideID = await _context.RoundSquads.Where(s => s.RoundSquadID == vm.RoundSquadID && s.Side.MatchSide.MatchID == id).Select(s => s.Side.MatchSideID).FirstOrDefaultAsync();
                    }
                    else if (vm.MatchSideID != null)
                    {
                        // Vérifie que MatchSideID appartient bien à MatchID
                        vm.MatchSideID = await _context.MatchSides.Where(s => s.MatchSideID == vm.MatchSideID && s.MatchID == id).Select(s => s.MatchSideID).FirstOrDefaultAsync();
                    }
                    else
                    {
                        if (await _context.MatchSides.CountAsync(s => s.MatchID == id) == 1)
                        {
                            vm.MatchSideID = (await _context.MatchSides.FirstAsync(s => s.MatchID == id)).MatchSideID;
                        }
                    }
                    matchUser = new MatchUser() { MatchID = id, UserID = vm.User.UserID, MatchSideID = vm.MatchSideID };
                    if (matchUser.MatchSideID == null || await CanJoin(matchUser))
                    {
                        _context.Add(matchUser);
                        await _context.SaveChangesAsync();
                    }
                }
                await transaction.CommitAsync();
            }
            return RedirectToAction(nameof(Subscription), new { id });
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSide(int id)
        {
            var user = await GetUser();
            if (user == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers.FirstOrDefaultAsync(u => u.MatchID == id && u.UserID == user.UserID);
            if (matchUser == null || matchUser.MatchSideID == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                matchUser.MatchSideID = null;
                _context.Update(matchUser);

                var olderSlots = await _context.RoundSlots.Where(s => s.MatchUserID == matchUser.MatchUserID).ToListAsync();
                foreach (var olderSlot in olderSlots)
                {
                    olderSlot.MatchUserID = null;
                    _context.Update(olderSlot);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            return RedirectToAction(nameof(Subscription), new { id });
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscriptionSlot(int id, int? roundSquadID, int? roundSlotID, int? roundSideID, string squadName)
        {
            var user = await GetUser();
            if (user == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers.Include(u => u.Side).FirstOrDefaultAsync(u => u.MatchID == id && u.UserID == user.UserID);
            if (matchUser == null || matchUser.MatchSideID == null)
            {
                return NotFound();
            }


            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                RoundSlot roundSlot = null;

                if (roundSlotID != null)
                {
                    roundSlot = await _context.RoundSlots.Include(s => s.Squad).ThenInclude(s => s.Side).FirstOrDefaultAsync(s => s.Squad.Side.Round.MatchID == id && s.Squad.Side.MatchSideID == matchUser.MatchSideID && s.RoundSlotID == roundSlotID);
                    if (roundSlot == null || roundSlot.MatchUserID != null)
                    {
                        return RedirectToAction(nameof(Subscription), new { id });
                    }
                }
                else if (roundSquadID != null)
                {
                    var roundSquad = await _context.RoundSquads.Include(s => s.Side).Include(s => s.Slots).FirstOrDefaultAsync(s => s.Side.Round.MatchID == id && s.Side.MatchSideID == matchUser.MatchSideID && s.RoundSquadID == roundSquadID);
                    if (roundSquad == null || roundSquad.RestrictTeamComposition)
                    {
                        return RedirectToAction(nameof(Subscription), new { id });
                    }

                    roundSlot = roundSquad.Slots.FirstOrDefault(s => s.MatchUserID == null);
                    if (roundSlot == null)
                    {
                        roundSquad.SlotsCount++;
                        roundSlot = new RoundSlot()
                        {
                            Squad = roundSquad,
                            SlotNumber = roundSquad.SlotsCount,
                            Role = Role.Member
                        };
                        roundSlot.SetTimestamp();
                        _context.Add(roundSlot);
                        _context.Update(roundSquad);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(squadName) && roundSideID != null)
                {
                    if (matchUser.Side.SquadsPolicy != SquadsPolicy.Unrestricted)
                    {
                        return RedirectToAction(nameof(Subscription), new { id });
                    }
                    var others = await _context.RoundSquads.Where(rs => rs.RoundSideID == roundSideID).ToListAsync();
                    var roundSquad = new RoundSquad()
                    {
                        Name = squadName,
                        UniqueDesignation = RoundSquad.UniqueDesignations.First(num => !others.Any(t => t.UniqueDesignation == num)),
                        RoundSideID = roundSideID.Value,
                        SlotsCount = 1,
                        Slots = new List<RoundSlot>(),
                        Side = await _context.RoundSides.FindAsync(roundSideID.Value),
                        RestrictTeamComposition = false,
                        InviteOnly = false
                    };
                    roundSlot = new RoundSlot()
                    {
                        Squad = roundSquad,
                        SlotNumber = 1,
                        Role = Role.SquadLeader
                    };
                    roundSlot.SetTimestamp();
                    _context.Add(roundSlot);
                    _context.Add(roundSquad);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest();
                }

                await AssignSlot(matchUser, roundSlot);

                await transaction.CommitAsync();
            }


            return RedirectToAction(nameof(Subscription), new { id });
        }

        private async Task AssignSlot(MatchUser matchUser, RoundSlot roundSlot)
        {
            var olderSlots = await _context.RoundSlots.Where(s => s.Squad.Side.RoundID == roundSlot.Squad.Side.RoundID && s.MatchUserID == matchUser.MatchUserID).ToListAsync();
            foreach (var olderSlot in olderSlots)
            {
                olderSlot.MatchUserID = null;
                _context.Update(olderSlot);
            }

            roundSlot.MatchUserID = matchUser.MatchUserID;
            roundSlot.IsValidated = false;
            if (roundSlot.Role == null)
            {
                roundSlot.Role = Role.Member;
            }
            _context.Update(roundSlot);
            await _context.SaveChangesAsync();
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveSlot(int id, int roundSlotID)
        {
            var user = await GetUser();
            if (user == null)
            {
                return NotFound();
            }

            var matchUser = await _context.MatchUsers.FirstOrDefaultAsync(u => u.MatchID == id && u.UserID == user.UserID);
            if (matchUser == null || matchUser.MatchSideID == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var roundSlot = await _context.RoundSlots.FirstOrDefaultAsync(s => s.RoundSlotID == roundSlotID);
                if (roundSlot.MatchUserID == matchUser.MatchUserID)
                {
                    roundSlot.MatchUserID = null;
                    _context.Update(roundSlot);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }

            return RedirectToAction(nameof(Subscription), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Map(int id, int? roundId)
        {
            var matchUser = await GetMatchUser(id);
            var vm = new MapViewModel();
            if (roundId == null)
            {
                var isAdmin = (await _auth.AuthorizeAsync(User, "Admin")).Succeeded;

                if (matchUser == null && !isAdmin)
                {
                    return RedirectToAction(nameof(Subscription), new { id });
                }

                // Carte de situation
                vm.Match = await _context.Matchs.Include(m => m.GameMap).Include(m => m.Rounds).FirstOrDefaultAsync(m => m.MatchID == id);

                if (vm.Match == null)
                {
                    return NotFound();
                }

                vm.CanEditMap = isAdmin;
                vm.MapId = new MapId() { matchID = vm.Match.MatchID };
            }
            else
            {
                if (matchUser == null)
                {
                    return RedirectToAction(nameof(Subscription), new { id });
                }

                // Carte partagée
                vm.Round = await _context.Rounds
                    .Include(m => m.Match).ThenInclude(m => m.GameMap)
                    .Include(m => m.Match).ThenInclude(m => m.Rounds)
                    .Include(m => m.Sides).ThenInclude(m => m.MatchSide)
                    .FirstOrDefaultAsync(m => m.RoundID == roundId && m.MatchID == id);
                if (vm.Round == null)
                {
                    return NotFound();
                }
                vm.Match = vm.Round.Match;
                vm.RoundSide = vm.Round.Sides.FirstOrDefault(s => s.MatchSideID == matchUser.MatchSideID);
                vm.MapId = new MapId()
                {
                    matchID = vm.Round.Match.MatchID,
                    roundSideID = vm.RoundSide.RoundSideID
                };
                vm.CanEditMap = await MapHub.CanUserEdit(_context, matchUser, vm.MapId);
            }

            // Vérifie qu'il y a un fond de carte
            if (string.IsNullOrEmpty(vm.Match.GameMap.WebMap))
            {
                return NotFound();
            }
            return View(vm);
        }

        private async Task<MatchUser> GetMatchUser(int id)
        {
            var user = await GetUser();
            if (user != null)
            {
                return await _context.MatchUsers.Include(u => u.Slots).FirstOrDefaultAsync(u => u.UserID == user.UserID && u.MatchID == id);
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var match = await _context.Matchs
                .Include(m => m.Sides)
                .Include(m => m.GameMap)
                .Include(m => m.MatchTechnicalInfos)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Squads).ThenInclude(s => s.Slots).ThenInclude(s => s.AssignedUser).ThenInclude(u => u.User)
                .Include(m => m.Rounds).ThenInclude(r => r.Sides).ThenInclude(s => s.Faction)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }
            match.News = await _context.News.Where(v => v.MatchID == id).OrderByDescending(v => v.Date).Take(1).ToListAsync();
            match.Videos = await _context.Videos.Where(v => v.MatchID == id).OrderBy(v => v.Date).ToListAsync();
            match.Documents = await _context.Documents.Where(v => v.MatchID == id).OrderBy(v => v.Date).ToListAsync();
            var vm = new EventDetailsViewModel();
            vm.Match = match;
            vm.User = await GetUser();
            if (vm.User != null)
            {
                vm.MatchUser = await GetMatchUser(match.MatchID);
            }
            AdminMatchsController.SortModel(match);
            return View(vm);
        }

        [Authorize(Policy = "LoggedUser")]
        [HttpGet]
        public async Task<IActionResult> DownloadModPack(int id)
        {
            var match = await _context.Matchs.Include(m => m.MatchTechnicalInfos).FirstOrDefaultAsync(m => m.MatchID == id);
            if (string.IsNullOrEmpty(match?.MatchTechnicalInfos?.ModsDefinition))
            {
                return NotFound();
            }
            var user = await GetMatchUser(id);
            if (user == null)
            {
                return Forbid();
            }
            if(!await _context.RoundSlots.AnyAsync(u => u.MatchUserID == user.MatchUserID && u.IsValidated))
            {
                return Forbid();
            }
            return File(Encoding.UTF8.GetBytes(match.MatchTechnicalInfos.ModsDefinition), "application/octet-steam", $"modpack{match.MatchID}.html");
        }

        [HttpGet]
        public IActionResult VoipSystem(VoipSystem id)
        {
            var name = Path.GetFileName(id.ToString());
            return View(new VoipSystemViewModel()
            {
                VoipSystem = id,
                HelpContent = System.IO.File.ReadAllText($"wwwroot/help/{name}.html")
            });
        }

        [HttpGet("/img/markers/{color}/{marker}.png")]
        [ResponseCache(Duration = 1440)]
        public IActionResult Icon(GameMarkerColor color, GameMarkerType marker)
        {
            if (color == GameMarkerColor.ColorWhite || marker >= GameMarkerType.flag_aaf)
            {
                return File($"/img/markers/{marker}.png", "image/png");
            }
            var targetColor = color.ToColor();
            using (var img = Image.Load<Rgba32>($"wwwroot/img/markers/{marker}.png"))
            {
                for(int x = 0; x < img.Width; ++x)
                {
                    for (int y = 0; y < img.Height; ++y)
                    {
                        var pixel = img[x, y];
                        img[x, y] = new Rgba32((byte)(pixel.R * targetColor[0]), (byte)(pixel.G * targetColor[1]), (byte)(pixel.B * targetColor[2]), pixel.A);
                    }
                }
                using (var stream = new MemoryStream())
                {
                    img.SaveAsPng(stream);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var user = await GetUser();
            if (user == null)
            {
                return NotFound();
            }
            var matchUser = await _context.MatchUsers
                .Include(u => u.User)
                .Include(u => u.Match)
                .FirstOrDefaultAsync(u => u.UserID == user.UserID && u.MatchID == id);
            if (matchUser == null)
            {
                return NotFound();
            }
            return View(matchUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("CancelSubscription")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSubscriptionConfirmed(int id)
        {
            var user = await GetUser();
            if (user == null)
            {
                return NotFound();
            }
            var matchUser = await _context.MatchUsers.FirstOrDefaultAsync(u => u.UserID == user.UserID && u.MatchID == id);
            if (matchUser == null)
            {
                return NotFound();
            }
            _context.MatchUsers.Remove(matchUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(HomeController.Index), ControllersName.Home);
        }

        public IActionResult Rules()
        {
            return View();
        }
    }
}