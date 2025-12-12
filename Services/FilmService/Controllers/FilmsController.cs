using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmService.Data;
using FilmService.Models;

namespace FilmService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmsController : ControllerBase
    {
        private readonly FilmDbContext _context;

        public FilmsController(FilmDbContext context)
        {
            _context = context;
        }

        // GET: api/Films
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Film>>> GetFilms(
            [FromQuery] string? genre = null,
            [FromQuery] string? recherche = null,
            [FromQuery] bool? actifSeulement = true)
        {
            var query = _context.Films.AsQueryable();

            if (actifSeulement == true)
            {
                query = query.Where(f => f.EstActif);
            }

            if (!string.IsNullOrEmpty(genre))
            {
                query = query.Where(f => f.Genre == genre);
            }

            if (!string.IsNullOrEmpty(recherche))
            {
                query = query.Where(f => f.Titre.Contains(recherche) || f.Description.Contains(recherche));
            }

            return await query.OrderByDescending(f => f.DateSortie).ToListAsync();
        }

        // GET: api/Films/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Film>> GetFilm(int id)
        {
            var film = await _context.Films
                .Include(f => f.Seances)
                    .ThenInclude(s => s.Salle)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (film == null)
            {
                return NotFound(new { message = "Film non trouvé" });
            }

            return film;
        }

        // GET: api/Films/genres
        [HttpGet("genres")]
        public async Task<ActionResult<IEnumerable<string>>> GetGenres()
        {
            var genres = await _context.Films
                .Where(f => f.EstActif)
                .Select(f => f.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            return genres;
        }

        // POST: api/Films
        [HttpPost]
        public async Task<ActionResult<Film>> PostFilm(Film film)
        {
            _context.Films.Add(film);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFilm), new { id = film.Id }, film);
        }

        // PUT: api/Films/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFilm(int id, Film film)
        {
            if (id != film.Id)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            _context.Entry(film).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilmExists(id))
                {
                    return NotFound(new { message = "Film non trouvé" });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Films/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFilm(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound(new { message = "Film non trouvé" });
            }

            // Soft delete - on désactive plutôt que supprimer
            film.EstActif = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
    }
}

