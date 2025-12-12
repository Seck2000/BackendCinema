using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmService.Data;
using FilmService.Models;

namespace FilmService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SallesController : ControllerBase
    {
        private readonly FilmDbContext _context;

        public SallesController(FilmDbContext context)
        {
            _context = context;
        }

        // GET: api/Salles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Salle>>> GetSalles([FromQuery] bool? activeSeulement = true)
        {
            var query = _context.Salles.AsQueryable();

            if (activeSeulement == true)
            {
                query = query.Where(s => s.EstActive);
            }

            return await query.OrderBy(s => s.Nom).ToListAsync();
        }

        // GET: api/Salles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Salle>> GetSalle(int id)
        {
            var salle = await _context.Salles
                .Include(s => s.Sieges)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salle == null)
            {
                return NotFound(new { message = "Salle non trouvée" });
            }

            return salle;
        }

        // GET: api/Salles/5/sieges
        [HttpGet("{id}/sieges")]
        public async Task<ActionResult<IEnumerable<Siege>>> GetSiegesBySalle(int id)
        {
            var salle = await _context.Salles.FindAsync(id);
            if (salle == null)
            {
                return NotFound(new { message = "Salle non trouvée" });
            }

            var sieges = await _context.Sieges
                .Where(s => s.SalleId == id)
                .OrderBy(s => s.Rang)
                .ThenBy(s => s.Numero)
                .ToListAsync();

            return sieges;
        }

        // POST: api/Salles
        [HttpPost]
        public async Task<ActionResult<Salle>> PostSalle(Salle salle)
        {
            _context.Salles.Add(salle);
            await _context.SaveChangesAsync();

            // Créer automatiquement les sièges pour la salle
            await CreateSiegesForSalle(salle);

            return CreatedAtAction(nameof(GetSalle), new { id = salle.Id }, salle);
        }

        // PUT: api/Salles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalle(int id, Salle salle)
        {
            if (id != salle.Id)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            _context.Entry(salle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalleExists(id))
                {
                    return NotFound(new { message = "Salle non trouvée" });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Salles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalle(int id)
        {
            var salle = await _context.Salles.FindAsync(id);
            if (salle == null)
            {
                return NotFound(new { message = "Salle non trouvée" });
            }

            salle.EstActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SalleExists(int id)
        {
            return _context.Salles.Any(e => e.Id == id);
        }

        private async Task CreateSiegesForSalle(Salle salle)
        {
            var sieges = new List<Siege>();
            var rangs = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            var siegesParRang = salle.NombrePlaces / rangs.Length;

            foreach (var rang in rangs)
            {
                for (int numero = 1; numero <= siegesParRang; numero++)
                {
                    sieges.Add(new Siege
                    {
                        SalleId = salle.Id,
                        Rang = rang,
                        Numero = numero,
                        Type = "Standard",
                        EstOccupe = false,
                        EstReserve = false
                    });
                }
            }

            _context.Sieges.AddRange(sieges);
            await _context.SaveChangesAsync();
        }
    }
}

