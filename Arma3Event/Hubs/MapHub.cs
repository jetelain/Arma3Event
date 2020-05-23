using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Arma3Event.Hubs
{
    [Authorize(Policy = "SteamID")]
    public class MapHub : Hub
    {
        private readonly Arma3EventContext _context;
        private readonly IAuthorizationService _auth;

        public MapHub(Arma3EventContext context, IAuthorizationService auth)
        {
            _context = context;
            _auth = auth;
        }

        private async Task<MatchUser> GetUser(int matchID)
        {
            var steamId = SteamHelper.GetSteamId(Context.User);
            var user = await _context.MatchUsers.FirstOrDefaultAsync(u => u.User.SteamId == steamId && u.MatchID == matchID);
            return user;
        }

        private async Task<bool> CanEdit(MatchUser user, int? roundSideID)
        {
            if (roundSideID == null)
            {
                // Seuls les organisateurs peuvent modifier la carte de situation
                return (await _auth.AuthorizeAsync(Context.User, "Admin")).Succeeded;
            }
            return await _context.RoundSides.AnyAsync(rs => rs.RoundSideID == roundSideID && rs.MatchSideID == user.MatchSideID);
        }

        private async Task<bool> CanRead(MatchUser user, int? roundSideID)
        {
            if (await _context.RoundSides.AnyAsync(rs => rs.RoundSideID == roundSideID && rs.MatchSideID == user.MatchSideID))
            {
                return true;
            }
            if (roundSideID == null && (await _auth.AuthorizeAsync(Context.User, "Admin")).Succeeded)
            {
                // Les organisateurs n'ont le droit de voir que la carte de situation
                return true;
            }
            return false;
        }

        public async Task Hello(int matchID, int? roundSideID)
        {
            var user = await GetUser(matchID);
            if (await CanRead(user, roundSideID))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Match:{matchID}");
                if (roundSideID != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"MatchRound:{roundSideID}");
                }
                var markers = _context.MapMarkers.Where(m => m.MatchID == matchID && (m.RoundSideID == null || m.RoundSideID == roundSideID)).ToList();
                foreach(var marker in markers)
                {
                    await Clients.Caller.SendAsync("AddOrUpdateMarker", ToJsonMarker(marker));
                }
            }
        }

        public async Task<int> AddMarker(int matchID, int? roundSideID, MarkerData markerData)
        {
            var user = await GetUser(matchID);
            if (await CanEdit(user, roundSideID))
            {
                var marker = new MapMarker()
                {
                    MatchID = matchID,
                    RoundSideID = roundSideID,
                    MarkerData = JsonConvert.SerializeObject(markerData),
                    MatchUserID = user?.MatchUserID
                };
                await _context.AddAsync(marker);
                await _context.SaveChangesAsync();
                await Notify("AddOrUpdateMarker", marker);
                return marker.MapMarkerID;
            }
            return 0;
        }

        public async Task UpdateMarker(int mapMarkerID, MarkerData markerData)
        {
            await DoUpdateMarker(mapMarkerID, markerData, true);
        }

        public async Task MoveMarker(int mapMarkerID, MarkerData markerData)
        {
            await DoUpdateMarker(mapMarkerID, markerData, false);
        }

        private async Task DoUpdateMarker(int mapMarkerID, MarkerData markerData, bool notifyCaller)
        {
            var marker = _context.MapMarkers.FirstOrDefault(m => m.MapMarkerID == mapMarkerID);
            var user = await GetUser(marker.MatchID);
            if (await CanEdit(user, marker.RoundSideID))
            {
                marker.MarkerData = JsonConvert.SerializeObject(markerData);
                _context.Update(marker);
                await _context.SaveChangesAsync();
                await Notify("AddOrUpdateMarker", marker, notifyCaller);
            }
        }

        public async Task RemoveMarker(int mapMarkerID)
        {
            var marker = _context.MapMarkers.FirstOrDefault(m => m.MapMarkerID == mapMarkerID);
            var user = await GetUser(marker.MatchID);
            if (await CanEdit(user, marker.RoundSideID))
            {
                _context.Remove(marker);
                await _context.SaveChangesAsync();
                await Notify("RemoveMarker", marker);
            }
        }

        public async Task PointMap(int matchID, int? roundSideID, double[] pos)
        {
            var user = await GetUser(matchID);
            
            if (await CanEdit(user, roundSideID))
            {
                if (roundSideID == null)
                {
                    await Clients.Group($"Match:{matchID}").SendAsync("PointMap", user.MatchUserID, pos);
                }
                else
                {
                    await Clients.Group($"MatchRound:{roundSideID}").SendAsync("PointMap", user.MatchUserID, pos);
                }
            }
        }

        public async Task EndPointMap(int matchID, int? roundSideID)
        {
            var user = await GetUser(matchID);
            if (await CanEdit(user, roundSideID))
            {
                if (roundSideID == null)
                {
                    await Clients.Group($"Match:{matchID}").SendAsync("EndPointMap", user.MatchUserID);
                }
                else
                {
                    await Clients.Group($"MatchRound:{roundSideID}").SendAsync("EndPointMap", user.MatchUserID);
                }
            }
        }

        private async Task Notify(string method, MapMarker marker, bool notifyCaller = true)
        {
            string groupName = marker.RoundSideID == null ? $"Match:{marker.MatchID}" : $"MatchRound:{marker.RoundSideID}";

            if (notifyCaller)
            {
                await Clients.Group(groupName).SendAsync(method, ToJsonMarker(marker));
            }
            else
            {
                await Clients.OthersInGroup(groupName).SendAsync(method, ToJsonMarker(marker));
            }
        }

        private static Marker ToJsonMarker(MapMarker marker)
        {
            return new Marker() 
            { 
                id = marker.MapMarkerID, 
                scope = marker.RoundSideID == null ? 0 : 1,
                data = JsonConvert.DeserializeObject<MarkerData>(marker.MarkerData) 
            };
        }
    }
}
