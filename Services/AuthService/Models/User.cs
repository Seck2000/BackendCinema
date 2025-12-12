using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User : IdentityUser
    {
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string TypeUtilisateur { get; set; } = "Client"; // "Client" ou "Fournisseur"
        
        [StringLength(200)]
        public string? NomEntreprise { get; set; } // Pour les fournisseurs
        
        [StringLength(200)]
        public string? DescriptionEntreprise { get; set; } // Pour les fournisseurs
        
        public bool EstActif { get; set; } = true;
        
        public DateTime DateInscription { get; set; } = DateTime.Now;
    }
}

