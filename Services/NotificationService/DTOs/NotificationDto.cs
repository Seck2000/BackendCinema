using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTOs
{
    public class EmailNotificationRequest
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;
    }

    public class SmsNotificationRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class ReservationConfirmationRequest
    {
        [Required]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        public string? CustomerPhone { get; set; }

        [Required]
        public string NumeroReservation { get; set; } = string.Empty;

        [Required]
        public string FilmTitre { get; set; } = string.Empty;

        [Required]
        public DateTime DateSeance { get; set; }

        [Required]
        public string SalleNom { get; set; } = string.Empty;

        public string? Sieges { get; set; }

        public int NombrePlaces { get; set; }

        public decimal MontantTotal { get; set; }
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NotificationId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}

