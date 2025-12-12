using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationService.Models
{
    public class PanierItem
    {
        public int Id { get; set; }
        
        public int SeanceId { get; set; }
        
        [Required]
        public string SessionId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public int NombrePlaces { get; set; }
        
        public int CategorieAgeId { get; set; }
        
        public int Quantite { get; set; }
        
        public decimal PrixUnitaire { get; set; }
        
        [NotMapped]
        public decimal PrixTotal => Quantite * PrixUnitaire;
        
        [StringLength(500)]
        public string? SiegeIds { get; set; }
        
        public DateTime DateAjout { get; set; } = DateTime.Now;
    }
}

