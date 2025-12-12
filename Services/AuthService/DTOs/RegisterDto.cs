using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TypeUtilisateur { get; set; } = "Client"; // "Client" ou "Fournisseur"

        [StringLength(200)]
        public string? NomEntreprise { get; set; }

        [StringLength(200)]
        public string? DescriptionEntreprise { get; set; }
    }
}

