using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        public int SeanceId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string NomClient { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EmailClient { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string TelephoneClient { get; set; } = string.Empty;
        
        public int NombrePlaces { get; set; }
        
        public int CategorieAgeId { get; set; }
        
        public decimal PrixUnitaire { get; set; }
        
        public decimal PrixTotal { get; set; }
        
        public DateTime DateReservation { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Statut { get; set; } = "EnAttente"; // EnAttente, Confirmee, Annulee
        
        [StringLength(200)]
        public string NumeroReservation { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? StripeChargeId { get; set; }
        
        [StringLength(500)]
        public string? SiegeIds { get; set; } // IDs des sièges réservés, séparés par des virgules
    }
}

