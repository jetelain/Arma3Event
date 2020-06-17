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
    public class MapHub : Hub
    {
        private readonly Arma3EventContext _context;
        private readonly IAuthorizationService _auth;

        public MapHub(Arma3EventContext context, IAuthorizationService auth)
        {
            _context = context;
            _auth = auth;
        }

        private async Task<MatchUser> GetUser(MapId mapId)
        {
            var user = await UserHelper.GetUser(_context, Context.User);
            if (user != null)
            {
                return await _context.MatchUsers.FirstOrDefaultAsync(u => u.UserID == user.UserID && u.MatchID == mapId.matchID);
            }
            return null;
        }

        private async Task<bool> CanEdit(MatchUser user, MapId mapId)
        {
            if (mapId.roundSideID == null)
            {
                // Seuls les organisateurs peuvent modifier la carte de situation
                return (await _auth.AuthorizeAsync(Context.User, "Admin")).Succeeded;
            }
            return await CanUserEdit(_context, user, mapId);
        }

        internal static async Task<bool> CanUserEdit(Arma3EventContext context, MatchUser user, MapId mapId)
        {
            return await context.RoundSlots.AnyAsync(rs => rs.Squad.RoundSideID == mapId.roundSideID &&
                rs.MatchUserID == user.MatchUserID &&
                rs.Role >= Role.SquadLeader &&
                rs.IsValidated);
        }

        private async Task<bool> CanRead(MatchUser user, MapId mapId)
        {
            if (mapId.roundSideID == null)
            {
                // Carte de situation, accessible à tous les inscripts et aux administrateurs
                //return user != null || (await _auth.AuthorizeAsync(Context.User, "Admin")).Succeeded;
                return true;
            }
            // Carte partagée, accessible uniquement aux inscrits du coté demandé
            return await _context.RoundSides.AnyAsync(rs => rs.RoundSideID == mapId.roundSideID && rs.MatchSideID == user.MatchSideID);
        }

        public async Task Hello(MapId mapId)
        {
            var user = await GetUser(mapId);
            if (await CanRead(user, mapId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Match:{mapId.matchID}");
                if (mapId.roundSideID != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"MatchRound:{mapId.roundSideID}");
                }
                var markers = _context.MapMarkers.Where(m => m.MatchID == mapId.matchID && (m.RoundSideID == null || m.RoundSideID == mapId.roundSideID)).ToList();
                foreach(var marker in markers)
                {
                    await Clients.Caller.SendAsync("AddOrUpdateMarker", ToJsonMarker(marker));
                }
            }
        }

        [Authorize(Policy = "LoggedUser")]
        public async Task<int> AddMarker(MapId mapId, MarkerData markerData)
        {
            var user = await GetUser(mapId);
            if (await CanEdit(user, mapId))
            {
                var marker = new MapMarker()
                {
                    MatchID = mapId.matchID,
                    RoundSideID = mapId.roundSideID,
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

        [Authorize(Policy = "LoggedUser")]
        public async Task UpdateMarker(int mapMarkerID, MarkerData markerData)
        {
            await DoUpdateMarker(mapMarkerID, markerData, true);
        }

        [Authorize(Policy = "LoggedUser")]
        public async Task MoveMarker(int mapMarkerID, MarkerData markerData)
        {
            await DoUpdateMarker(mapMarkerID, markerData, false);
        }

        private async Task DoUpdateMarker(int mapMarkerID, MarkerData markerData, bool notifyCaller)
        {
            var marker = _context.MapMarkers.FirstOrDefault(m => m.MapMarkerID == mapMarkerID);
            if (marker != null)
            {
                var mapId = new MapId(marker);
                var user = await GetUser(mapId);
                if (await CanEdit(user, mapId))
                {
                    marker.MarkerData = JsonConvert.SerializeObject(markerData);
                    marker.MatchUserID = user?.MatchUserID;
                    _context.Update(marker);
                    await _context.SaveChangesAsync();
                    await Notify("AddOrUpdateMarker", marker, notifyCaller);
                }
            }
        }

        [Authorize(Policy = "LoggedUser")]
        public async Task RemoveMarker(int mapMarkerID)
        {
            var marker = _context.MapMarkers.FirstOrDefault(m => m.MapMarkerID == mapMarkerID);
            if (marker != null)
            {
                var mapId = new MapId(marker);
                var user = await GetUser(mapId);
                if (await CanEdit(user, mapId))
                {
                    _context.Remove(marker);
                    await _context.SaveChangesAsync();
                    await Notify("RemoveMarker", marker);
                }
            }
        }

        [Authorize(Policy = "LoggedUser")]
        public async Task PointMap(MapId mapId, double[] pos)
        {
            var user = await GetUser(mapId);
            if (await CanEdit(user, mapId))
            {
                await Clients.Group(mapId.GetGroup()).SendAsync("PointMap", user.MatchUserID, pos);
            }
        }

        [Authorize(Policy = "LoggedUser")]
        public async Task EndPointMap(MapId mapId)
        {
            var user = await GetUser(mapId);
            if (await CanEdit(user, mapId))
            {
                await Clients.Group(mapId.GetGroup()).SendAsync("EndPointMap", user.MatchUserID);
            }
        }

        private async Task Notify(string method, MapMarker marker, bool notifyCaller = true)
        {
            MapId mapId = new MapId(marker);
            if (notifyCaller)
            {
                await Clients.Group(mapId.GetGroup()).SendAsync(method, ToJsonMarker(marker));
            }
            else
            {
                await Clients.OthersInGroup(mapId.GetGroup()).SendAsync(method, ToJsonMarker(marker));
            }
        }

        private static Marker ToJsonMarker(MapMarker marker)
        {
            return new Marker() 
            { 
                id = marker.MapMarkerID, 
                mapId = new MapId(marker),
                data = JsonConvert.DeserializeObject<MarkerData>(marker.MarkerData) 
            };
        }
    }
}
