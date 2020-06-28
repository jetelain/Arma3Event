using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Xml;
using HtmlAgilityPack;
using System.Net.Http;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "VideoSpecialist")]
    public class AdminVideosController : Controller
    {
        private readonly Arma3EventContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public AdminVideosController(Arma3EventContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        // GET: AdminVideos
        public async Task<IActionResult> Index()
        {
            var arma3EventContext = _context.Videos.Include(v => v.Match);
            return View(await arma3EventContext.ToListAsync());
        }

        // GET: AdminVideos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Videos
                .Include(v => v.Match)
                .FirstOrDefaultAsync(m => m.VideoID == id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        // GET: AdminVideos/Create
        public IActionResult Create()
        {
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name");
            return View();
        }

        // POST: AdminVideos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VideoID,VideoLink,Title,Image,Date,MatchID")] Video video)
        {
            if (ModelState.IsValid)
            {
                video.Date = DateTime.Now;

                if(string.IsNullOrEmpty(video.Title) || string.IsNullOrEmpty(video.Image))
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, video.VideoLink);
                    var client = _clientFactory.CreateClient();
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(await response.Content.ReadAsStringAsync());

                        video.Image = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", "");
                        video.Title = doc.DocumentNode.SelectSingleNode("//meta[@name='title']")?.GetAttributeValue("content", "");
                    }
                }

                _context.Add(video);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name", video.MatchID);
            return View(video);
        }

        // GET: AdminVideos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name", video.MatchID);
            return View(video);
        }

        // POST: AdminVideos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VideoID,VideoLink,Title,Image,Date,MatchID")] Video video)
        {
            if (id != video.VideoID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(video);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VideoExists(video.VideoID))
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
            ViewData["MatchID"] = new SelectList(_context.Matchs, "MatchID", "Name", video.MatchID);
            return View(video);
        }

        // GET: AdminVideos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Videos
                .Include(v => v.Match)
                .FirstOrDefaultAsync(m => m.VideoID == id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        // POST: AdminVideos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VideoExists(int id)
        {
            return _context.Videos.Any(e => e.VideoID == id);
        }
    }
}
