using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models
{
    public class Facture
    {
        public int Id { get; set; }
        
        [Required]
        public string NumeroFacture { get; set; } = string.Empty;
        
        [Required]
        public int ReservationId { get; set; }
        
        [Required]
        public string ClientId { get; set; } = string.Empty;
        
        public string? FournisseurId { get; set; }
        
        [Required]
        public decimal Montant { get; set; }
        
        [Required]
        public DateTime DateFacture { get; set; } = DateTime.Now;
        
        [Required]
        public string Statut { get; set; } = "Payée";
        
        public string? PaymentIntentId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NomClient { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string EmailClient { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? AdresseClient { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NomFournisseur { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string EmailFournisseur { get; set; } = string.Empty;
    }
}

