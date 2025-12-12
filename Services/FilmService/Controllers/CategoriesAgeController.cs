using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmService.Data;
using FilmService.Models;

namespace FilmService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesAgeController : ControllerBase
    {
        private readonly FilmDbContext _context;

        public CategoriesAgeController(FilmDbContext context)
        {
            _context = context;
        }

        // GET: api/CategoriesAge
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategorieAge>>> GetCategoriesAge([FromQuery] bool? activeSeulement = true)
        {
            var query = _context.CategoriesAge.AsQueryable();

            if (activeSeulement == true)
            {
                query = query.Where(c => c.EstActive);
            }

            return await query.OrderBy(c => c.Prix).ToListAsync();
        }

        // GET: api/CategoriesAge/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategorieAge>> GetCategorieAge(int id)
        {
            var categorieAge = await _context.CategoriesAge.FindAsync(id);

            if (categorieAge == null)
            {
                return NotFound(new { message = "Catégorie d'âge non trouvée" });
            }

            return categorieAge;
        }

        // POST: api/CategoriesAge
        [HttpPost]
        public async Task<ActionResult<CategorieAge>> PostCategorieAge(CategorieAge categorieAge)
        {
            _context.CategoriesAge.Add(categorieAge);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategorieAge), new { id = categorieAge.Id }, categorieAge);
        }

        // PUT: api/CategoriesAge/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategorieAge(int id, CategorieAge categorieAge)
        {
            if (id != categorieAge.Id)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            _context.Entry(categorieAge).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategorieAgeExists(id))
                {
                    return NotFound(new { message = "Catégorie d'âge non trouvée" });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/CategoriesAge/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategorieAge(int id)
        {
            var categorieAge = await _context.CategoriesAge.FindAsync(id);
            if (categorieAge == null)
            {
                return NotFound(new { message = "Catégorie d'âge non trouvée" });
            }

            categorieAge.EstActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategorieAgeExists(int id)
        {
            return _context.CategoriesAge.Any(e => e.Id == id);
        }
    }
}

