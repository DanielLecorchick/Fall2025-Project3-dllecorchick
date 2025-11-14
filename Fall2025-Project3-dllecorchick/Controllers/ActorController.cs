using Fall2025_Project3_dllecorchick.Data;
using Fall2025_Project3_dllecorchick.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_dllecorchick.AI;

namespace Fall2025_Project3_dllecorchick.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAi;

        public ActorsController(ApplicationDbContext context, OpenAIService openAi)
        {
            _context = context;
            _openAi = openAi;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                    .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
                return NotFound();

            // 🧠 Generate AI tweets about the actor
            var posts = await _openAi.GenerateActorPostsAsync(actor.Name);
            double avgSentiment = posts.Any() ? posts.Average(p => p.Sentiment) : 0.0;

            var viewModel = new ActorDetailsModel
            {
                Actor = actor,
                Posts = posts,
                AverageSentiment = avgSentiment
            };

            return View(viewModel);
        }


        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await Photo.CopyToAsync(stream);
                    actor.Photo = stream.ToArray();
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
                return NotFound();

            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor, IFormFile? Photo)
        {
            if (id != actor.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingActor = await _context.Actors.FindAsync(id);
                if (existingActor == null)
                    return NotFound();

                // Update text fields
                existingActor.Name = actor.Name;
                existingActor.Gender = actor.Gender;
                existingActor.Age = actor.Age;
                existingActor.IMDBLink = actor.IMDBLink;

                // Update photo if uploaded
                if (Photo != null && Photo.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await Photo.CopyToAsync(ms);
                    existingActor.Photo = ms.ToArray();
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }


        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null)
                return NotFound();

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
                _context.Actors.Remove(actor);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }

        public IActionResult Photo(int id)
        {
            var actor = _context.Actors.Find(id);
            if (actor?.Photo == null)
                return NotFound();

            byte[] bytes = actor.Photo;

            string mimeType = "image/jpeg";

            if (bytes.Length > 4)
            {
                if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                    mimeType = "image/png";

                else if (bytes.Length > 12 &&
                         bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                         bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                    mimeType = "image/webp";
            }

            return File(bytes, mimeType);
        }
    }
}