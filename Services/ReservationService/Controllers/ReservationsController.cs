using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Models;

namespace ReservationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationDbContext _context;

        public ReservationsController(ReservationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(
            [FromQuery] string? userId = null,
            [FromQuery] int? seanceId = null,
            [FromQuery] string? statut = null)
        {
            var query = _context.Reservations.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(r => r.UserId == userId);
            }

            if (seanceId.HasValue)
            {
                query = query.Where(r => r.SeanceId == seanceId.Value);
            }

            if (!string.IsNullOrEmpty(statut))
            {
                query = query.Where(r => r.Statut == statut);
            }

            return await query.OrderByDescending(r => r.DateReservation).ToListAsync();
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound(new { message = "Réservation non trouvée" });
            }

            return reservation;
        }

        // GET: api/Reservations/numero/{numeroReservation}
        [HttpGet("numero/{numeroReservation}")]
        public async Task<ActionResult<Reservation>> GetReservationByNumero(string numeroReservation)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.NumeroReservation == numeroReservation);

            if (reservation == null)
            {
                return NotFound(new { message = "Réservation non trouvée" });
            }

            return reservation;
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            // Générer un numéro de réservation unique
            reservation.NumeroReservation = GenerateReservationNumber();
            reservation.DateReservation = DateTime.Now;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/Reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound(new { message = "Réservation non trouvée" });
                }
                throw;
            }

            return NoContent();
        }

        // PUT: api/Reservations/5/confirmer
        [HttpPut("{id}/confirmer")]
        public async Task<IActionResult> ConfirmerReservation(int id, [FromBody] string? stripeChargeId = null)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Réservation non trouvée" });
            }

            reservation.Statut = "Confirmee";
            if (!string.IsNullOrEmpty(stripeChargeId))
            {
                reservation.StripeChargeId = stripeChargeId;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Réservation confirmée", reservation });
        }

        // PUT: api/Reservations/5/annuler
        [HttpPut("{id}/annuler")]
        public async Task<IActionResult> AnnulerReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Réservation non trouvée" });
            }

            reservation.Statut = "Annulee";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Réservation annulée", reservation });
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Réservation non trouvée" });
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        private string GenerateReservationNumber()
        {
            return $"RES{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}

