using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmService.Data;
using FilmService.Models;

namespace FilmService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeancesController : ControllerBase
    {
        private readonly FilmDbContext _context;

        public SeancesController(FilmDbContext context)
        {
            _context = context;
        }

        // GET: api/Seances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seance>>> GetSeances(
            [FromQuery] int? filmId = null,
            [FromQuery] int? salleId = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null,
            [FromQuery] bool? activeSeulement = true)
        {
            var query = _context.Seances
                .Include(s => s.Film)
                .Include(s => s.Salle)
                .AsQueryable();

            if (activeSeulement == true)
            {
                query = query.Where(s => s.EstActive && s.DateHeure > DateTime.Now);
            }

            if (filmId.HasValue)
            {
                query = query.Where(s => s.FilmId == filmId.Value);
            }

            if (salleId.HasValue)
            {
                query = query.Where(s => s.SalleId == salleId.Value);
            }

            if (dateDebut.HasValue)
            {
                query = query.Where(s => s.DateHeure >= dateDebut.Value);
            }

            if (dateFin.HasValue)
            {
                query = query.Where(s => s.DateHeure <= dateFin.Value);
            }

            return await query.OrderBy(s => s.DateHeure).ToListAsync();
        }

        // GET: api/Seances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Seance>> GetSeance(int id)
        {
            var seance = await _context.Seances
                .Include(s => s.Film)
                .Include(s => s.Salle)
                    .ThenInclude(sa => sa.Sieges)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seance == null)
            {
                return NotFound(new { message = "Séance non trouvée" });
            }

            return seance;
        }

        // GET: api/Seances/5/disponibilite
        [HttpGet("{id}/disponibilite")]
        public async Task<ActionResult<object>> GetDisponibilite(int id)
        {
            var seance = await _context.Seances
                .Include(s => s.Salle)
                    .ThenInclude(sa => sa.Sieges)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seance == null)
            {
                return NotFound(new { message = "Séance non trouvée" });
            }

            var siegesDisponibles = seance.Salle.Sieges
                .Count(s => !s.EstOccupe && !s.EstReserve);

            return new
            {
                SeanceId = id,
                PlacesTotales = seance.Salle.NombrePlaces,
                PlacesDisponibles = siegesDisponibles,
                PlacesOccupees = seance.Salle.NombrePlaces - siegesDisponibles
            };
        }

        // POST: api/Seances
        [HttpPost]
        public async Task<ActionResult<Seance>> PostSeance(Seance seance)
        {
            // Vérifier que le film existe
            var film = await _context.Films.FindAsync(seance.FilmId);
            if (film == null)
            {
                return BadRequest(new { message = "Film non trouvé" });
            }

            // Vérifier que la salle existe
            var salle = await _context.Salles.FindAsync(seance.SalleId);
            if (salle == null)
            {
                return BadRequest(new { message = "Salle non trouvée" });
            }

            // Vérifier qu'il n'y a pas de conflit d'horaire
            var dureeFilm = film.Duree;
            var finSeance = seance.DateHeure.AddMinutes(dureeFilm + 30); // 30 min de nettoyage

            var conflit = await _context.Seances
                .AnyAsync(s => s.SalleId == seance.SalleId 
                    && s.EstActive 
                    && s.DateHeure < finSeance 
                    && s.DateHeure.AddMinutes(dureeFilm + 30) > seance.DateHeure);

            if (conflit)
            {
                return BadRequest(new { message = "Il y a un conflit d'horaire avec une autre séance" });
            }

            _context.Seances.Add(seance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSeance), new { id = seance.Id }, seance);
        }

        // PUT: api/Seances/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSeance(int id, Seance seance)
        {
            if (id != seance.Id)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            _context.Entry(seance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeanceExists(id))
                {
                    return NotFound(new { message = "Séance non trouvée" });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Seances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeance(int id)
        {
            var seance = await _context.Seances.FindAsync(id);
            if (seance == null)
            {
                return NotFound(new { message = "Séance non trouvée" });
            }

            seance.EstActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SeanceExists(int id)
        {
            return _context.Seances.Any(e => e.Id == id);
        }
    }
}

