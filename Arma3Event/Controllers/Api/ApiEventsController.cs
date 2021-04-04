using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3TacMapLibrary.TacMaps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arma3Event.Controllers.Api
{
    [Route("api/events")]
    [ApiController]
    [Authorize(Policy = "ApiClient")]
    public class ApiEventsController : ControllerBase
    {
        private readonly Arma3EventContext _context;
        private readonly IApiTacMaps _tacMaps;

        public ApiEventsController(Arma3EventContext context, IApiTacMaps tacMaps)
        {
            _context = context;
            _tacMaps = tacMaps;
        }

        [HttpGet]
        public async Task<IEnumerable<ApiEvent>> GetEvents()
        {
            return (await Task.WhenAll((await _context.Matchs.OrderByDescending(e => e.StartDate).ToListAsync()).Select(ToEventApiAsync))).ToList();
        }

        private async Task<ApiEvent> ToEventApiAsync(Match match)
        {
            return new ApiEvent()
            {
                Description = match.Description,
                DetailsHref = Url.Action(nameof(EventsController.Details), "Events", new { id = match.MatchID }, Request.Scheme),
                End = match.StartDate.AddHours(4),
                Id = match.MatchID,
                Image = string.IsNullOrEmpty(match.Image) ? "" : new Uri(new Uri(Request.GetEncodedUrl()),match.Image).AbsoluteUri,
                Title = match.Name,
                Start = match.StartDate,
                SubscribeHref = Url.Action(nameof(EventsController.Subscription), "Events", new { id = match.MatchID }, Request.Scheme),
                Summary = match.ShortDescription,
                AdditionalLinks = await CreateAdditionalLinks(match)
            };
        }

        private async Task<string[]> CreateAdditionalLinks(Match match)
        {
            if(match.TacMapId != null)
            {
                var tacMap = await _tacMaps.Get(match.TacMapId.Value);
                if (tacMap != null)
                {
                    return new[] { tacMap.ReadOnlyHref };
                }
            }
            return null;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var match = await _context.Matchs.Where(m => m.MatchID == id).FirstOrDefaultAsync();
            if (match == null)
            {
                return NotFound();
            }
            return Ok(await ToEventApiAsync(match));
        }

        [HttpGet("{id}/personnels")]
        public async Task<IActionResult> GetEventPersonnels(int id)
        {
            var match = await _context.Matchs
                .Where(m => m.MatchID == id)
                .Include(m => m.Users).ThenInclude(u => u.User)
                .FirstOrDefaultAsync();
            if (match == null)
            {
                return NotFound();
            }
            return Ok(match.Users.Select(u => new ApiEventPersonnel()
            {
                SteamId = u.User.SteamId,
                Name = u.User.Name 
            }).ToList());
        }
    }
}
