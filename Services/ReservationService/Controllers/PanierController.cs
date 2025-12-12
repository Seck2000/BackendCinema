using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Models;

namespace ReservationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PanierController : ControllerBase
    {
        private readonly ReservationDbContext _context;

        public PanierController(ReservationDbContext context)
        {
            _context = context;
        }

        // GET: api/Panier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PanierItem>>> GetPanierItems(
            [FromQuery] string? sessionId = null,
            [FromQuery] string? userId = null)
        {
            var query = _context.PanierItems.AsQueryable();

            if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(p => p.SessionId == sessionId);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.UserId == userId);
            }

            return await query.OrderByDescending(p => p.DateAjout).ToListAsync();
        }

        // GET: api/Panier/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PanierItem>> GetPanierItem(int id)
        {
            var panierItem = await _context.PanierItems.FindAsync(id);

            if (panierItem == null)
            {
                return NotFound(new { message = "Article du panier non trouvé" });
            }

            return panierItem;
        }

        // GET: api/Panier/total
        [HttpGet("total")]
        public async Task<ActionResult<object>> GetPanierTotal(
            [FromQuery] string? sessionId = null,
            [FromQuery] string? userId = null)
        {
            var query = _context.PanierItems.AsQueryable();

            if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(p => p.SessionId == sessionId);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.UserId == userId);
            }

            var items = await query.ToListAsync();
            var total = items.Sum(p => p.Quantite * p.PrixUnitaire);
            var nombreArticles = items.Sum(p => p.Quantite);

            return new
            {
                NombreArticles = nombreArticles,
                Total = total,
                Items = items
            };
        }

        // POST: api/Panier
        [HttpPost]
        public async Task<ActionResult<PanierItem>> PostPanierItem(PanierItem panierItem)
        {
            panierItem.DateAjout = DateTime.Now;

            _context.PanierItems.Add(panierItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPanierItem), new { id = panierItem.Id }, panierItem);
        }

        // PUT: api/Panier/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPanierItem(int id, PanierItem panierItem)
        {
            if (id != panierItem.Id)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            _context.Entry(panierItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PanierItemExists(id))
                {
                    return NotFound(new { message = "Article du panier non trouvé" });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Panier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePanierItem(int id)
        {
            var panierItem = await _context.PanierItems.FindAsync(id);
            if (panierItem == null)
            {
                return NotFound(new { message = "Article du panier non trouvé" });
            }

            _context.PanierItems.Remove(panierItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Panier/vider
        [HttpDelete("vider")]
        public async Task<IActionResult> ViderPanier(
            [FromQuery] string? sessionId = null,
            [FromQuery] string? userId = null)
        {
            var query = _context.PanierItems.AsQueryable();

            if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(p => p.SessionId == sessionId);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.UserId == userId);
            }

            var items = await query.ToListAsync();
            _context.PanierItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Panier vidé", itemsSupprimes = items.Count });
        }

        private bool PanierItemExists(int id)
        {
            return _context.PanierItems.Any(e => e.Id == id);
        }
    }
}

