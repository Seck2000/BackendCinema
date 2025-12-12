using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ILogger<NotificationsController> logger)
        {
            _logger = logger;
        }

        // POST: api/Notifications/email
        [HttpPost("email")]
        public ActionResult<NotificationResponse> SendEmail([FromBody] EmailNotificationRequest request)
        {
            // Simulation d'envoi d'email (fictif pour le TP)
            var notificationId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "📧 EMAIL FICTIF ENVOYÉ:\n" +
                $"   À: {request.To}\n" +
                $"   Sujet: {request.Subject}\n" +
                $"   Corps: {request.Body}\n" +
                $"   ID: {notificationId}"
            );

            return Ok(new NotificationResponse
            {
                Success = true,
                Message = $"Email fictif envoyé à {request.To}",
                NotificationId = notificationId,
                SentAt = DateTime.Now
            });
        }

        // POST: api/Notifications/sms
        [HttpPost("sms")]
        public ActionResult<NotificationResponse> SendSms([FromBody] SmsNotificationRequest request)
        {
            // Simulation d'envoi de SMS (fictif pour le TP)
            var notificationId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "📱 SMS FICTIF ENVOYÉ:\n" +
                $"   À: {request.PhoneNumber}\n" +
                $"   Message: {request.Message}\n" +
                $"   ID: {notificationId}"
            );

            return Ok(new NotificationResponse
            {
                Success = true,
                Message = $"SMS fictif envoyé à {request.PhoneNumber}",
                NotificationId = notificationId,
                SentAt = DateTime.Now
            });
        }

        // POST: api/Notifications/reservation-confirmation
        [HttpPost("reservation-confirmation")]
        public ActionResult<NotificationResponse> SendReservationConfirmation([FromBody] ReservationConfirmationRequest request)
        {
            var notificationId = Guid.NewGuid().ToString();

            // Générer le contenu de l'email
            var emailBody = GenerateReservationConfirmationEmail(request);

            _logger.LogInformation(
                "🎬 CONFIRMATION DE RÉSERVATION ENVOYÉE:\n" +
                $"   À: {request.CustomerEmail}\n" +
                $"   Client: {request.CustomerName}\n" +
                $"   Réservation: {request.NumeroReservation}\n" +
                $"   Film: {request.FilmTitre}\n" +
                $"   Date: {request.DateSeance:dd/MM/yyyy HH:mm}\n" +
                $"   Salle: {request.SalleNom}\n" +
                $"   Places: {request.NombrePlaces}\n" +
                $"   Montant: {request.MontantTotal:C}\n" +
                $"   ID: {notificationId}"
            );

            // Envoyer aussi un SMS si le numéro est fourni
            if (!string.IsNullOrEmpty(request.CustomerPhone))
            {
                _logger.LogInformation(
                    "📱 SMS DE CONFIRMATION:\n" +
                    $"   À: {request.CustomerPhone}\n" +
                    $"   Message: Réservation {request.NumeroReservation} confirmée pour {request.FilmTitre}"
                );
            }

            return Ok(new NotificationResponse
            {
                Success = true,
                Message = $"Confirmation de réservation envoyée à {request.CustomerEmail}",
                NotificationId = notificationId,
                SentAt = DateTime.Now
            });
        }

        // GET: api/Notifications/test
        [HttpGet("test")]
        public ActionResult<NotificationResponse> TestNotification()
        {
            _logger.LogInformation("🔔 Test de notification réussi!");

            return Ok(new NotificationResponse
            {
                Success = true,
                Message = "Service de notification opérationnel",
                NotificationId = Guid.NewGuid().ToString(),
                SentAt = DateTime.Now
            });
        }

        private string GenerateReservationConfirmationEmail(ReservationConfirmationRequest request)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #1a1a2e; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .details {{ background-color: #f4f4f4; padding: 15px; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎬 CinéReserv</h1>
            <h2>Confirmation de réservation</h2>
        </div>
        <div class='content'>
            <p>Bonjour <strong>{request.CustomerName}</strong>,</p>
            <p>Votre réservation a été confirmée avec succès!</p>
            
            <div class='details'>
                <p><strong>Numéro de réservation:</strong> {request.NumeroReservation}</p>
                <p><strong>Film:</strong> {request.FilmTitre}</p>
                <p><strong>Date et heure:</strong> {request.DateSeance:dddd dd MMMM yyyy à HH:mm}</p>
                <p><strong>Salle:</strong> {request.SalleNom}</p>
                <p><strong>Sièges:</strong> {request.Sieges ?? "Non attribués"}</p>
                <p><strong>Nombre de places:</strong> {request.NombrePlaces}</p>
                <p><strong>Montant total:</strong> {request.MontantTotal:C}</p>
            </div>
            
            <p>Merci de présenter ce numéro de réservation à l'entrée.</p>
        </div>
        <div class='footer'>
            <p>CinéReserv - Votre cinéma en ligne</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}

