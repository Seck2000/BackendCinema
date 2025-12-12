using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs
{
    public class PaymentRequest
    {
        [Required]
        public decimal Amount { get; set; } // Montant en dollars (sera converti en cents pour Stripe)

        [Required]
        public string Currency { get; set; } = "cad"; // Devise (CAD par défaut)

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }

        public string? CustomerName { get; set; }

        public int? ReservationId { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class PaymentIntentRequest
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "cad";

        public string? CustomerEmail { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public string? ChargeId { get; set; }
        public string? Status { get; set; }
    }
}

