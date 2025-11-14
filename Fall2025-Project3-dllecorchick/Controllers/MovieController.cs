using Fall2025_Project3_dllecorchick.AI;
using Fall2025_Project3_dllecorchick.Data;
using Fall2025_Project3_dllecorchick.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fall2025_Project3_dllecorchick.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAi;

        public MoviesController(ApplicationDbContext context, OpenAIService openAi)
        {
            _context = context;
            _openAi = openAi;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            var reviews = await _openAi.GenerateReviewsAsync(movie.Title, movie.Year.ToString(), "Unknown Director");
            double avgSentiment = reviews.Average(r => r.Sentiment);

            var viewModel = new MovieDetailsModel
            {
                Movie = movie,
                Reviews = reviews,
                AverageSentiment = avgSentiment
            };

            return View(viewModel);
        }


        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDBLink,Genre,Year")] Movie movie, IFormFile? Poster)
        {
            if (ModelState.IsValid)
            {
                if (Poster != null && Poster.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await Poster.CopyToAsync(stream);
                    movie.Poster = stream.ToArray();
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDBLink,Genre,Year")] Movie movie, IFormFile? Poster)
        {
            if (id != movie.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingMovie = await _context.Movies.FindAsync(id);
                if (existingMovie == null)
                    return NotFound();

                // Update text fields
                existingMovie.Title = movie.Title;
                existingMovie.Genre = movie.Genre;
                existingMovie.Year = movie.Year;
                existingMovie.IMDBLink = movie.IMDBLink;

                // Update poster if a new file was uploaded
                if (Poster != null && Poster.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await Poster.CopyToAsync(stream);
                    existingMovie.Poster = stream.ToArray();
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }


        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
                _context.Movies.Remove(movie);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        public async Task<IActionResult> PosterImage(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null || movie.Poster == null)
                return NotFound();

            string mimeType = "image/jpeg";

            var data = movie.Poster;

            if (data.Length > 4)
            {
                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E)
                    mimeType = "image/png";
                else if (data[8] == 0x57 && data[9] == 0x45)
                    mimeType = "image/webp";
            }
            return File(data, mimeType);
        }
    }
}

