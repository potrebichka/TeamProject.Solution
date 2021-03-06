  
using Microsoft.AspNetCore.Mvc;
using NUE.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using NUE.Data;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NUE.Controllers
{
     public class EventsController : Controller
     {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EventsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        public ActionResult Index()
        {
            List<Event> model = _db.Events.ToList();
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Event musicEvent)
        {
            _db.Events.Add(musicEvent);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

         public ActionResult Details(int id)
        {
            var thisEvent = _db.Events
                .Include(ev => ev.Comments)
                .ThenInclude(comment => comment.User)
                .FirstOrDefault(musicEvent => musicEvent.EventId == id);
            thisEvent.Comments =thisEvent.Comments.OrderBy(c => c.Time).ToList();
            return View(thisEvent);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.EventId = new SelectList(_db.Events, "EventId", "EventTitle");
            var thisEvent = _db.Events.FirstOrDefault(musicEvent => musicEvent.EventId == id);
            return View(thisEvent);
        }

        [HttpPost]
        public ActionResult Edit(Event musicEvent)
        {
        _db.Entry(musicEvent).State = EntityState.Modified;
        _db.SaveChanges();
        return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
        var thisEvent = _db.Events.FirstOrDefault(musicEvent => musicEvent.EventId == id);
        return View(thisEvent);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
        var thisEvent = _db.Events.FirstOrDefault(musicEvent => musicEvent.EventId == id);
        _db.Events.Remove(thisEvent);
        _db.SaveChanges();
        return RedirectToAction("Index");
        }

        public ActionResult DeleteAll()
        {
        var allEvents = _db.Events.ToList();
        return View();
        }

        [HttpPost, ActionName("DeleteAll")]
        public ActionResult DeleteAllConfirmed()
        {
        var allEvents = _db.Events.ToList();

        foreach (var musicEvent in allEvents)
        {
            _db.Events.Remove(musicEvent);
        }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> AddComment(Comment comment, int Eventid)
        {

            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);
            var image = "http://nicesnippets.com/demo/man01.png";
            var googleClaim = this.User.Claims.FirstOrDefault(c => c.Type == "urn:google:picture");
            var facebookClaim = this.User.Claims.FirstOrDefault(c => c.Type == "Picture");
            var twitterClaim = this.User.Claims.FirstOrDefault(c => c.Type == "profile-image-url");
            if ( googleClaim != null)
            {
                image = googleClaim.Value;
            }
            if ( facebookClaim != null)
            {
                image = facebookClaim.Value;
            }

            if ( twitterClaim != null)
            {
                image = twitterClaim.Value;
            }
            comment.Image = image;
            comment.User = currentUser;
            comment.Time = DateTime.Now;
            _db.Comments.Add(comment);
            _db.SaveChanges();
            return RedirectToAction("Details", new {id = Eventid});
        }
    }
}