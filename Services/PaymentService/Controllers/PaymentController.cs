using Microsoft.AspNetCore.Mvc;
using Stripe;
using PaymentService.DTOs;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        // POST: api/Payment/create-payment-intent
        [HttpPost("create-payment-intent")]
        public async Task<ActionResult<PaymentResponse>> CreatePaymentIntent([FromBody] PaymentIntentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convertir en cents
                    Currency = request.Currency,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = request.Metadata ?? new Dictionary<string, string>()
                };

                if (!string.IsNullOrEmpty(request.CustomerEmail))
                {
                    options.ReceiptEmail = request.CustomerEmail;
                }

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return Ok(new PaymentResponse
                {
                    Success = true,
                    Message = "PaymentIntent créé avec succès",
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Erreur Stripe: {ex.Message}"
                });
            }
        }

        // POST: api/Payment/charge
        [HttpPost("charge")]
        public async Task<ActionResult<PaymentResponse>> CreateCharge([FromBody] PaymentRequest request)
        {
            try
            {
                // Créer d'abord un PaymentIntent
                var intentOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100),
                    Currency = request.Currency,
                    Description = request.Description,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = request.Metadata ?? new Dictionary<string, string>()
                };

                if (request.ReservationId.HasValue)
                {
                    intentOptions.Metadata["ReservationId"] = request.ReservationId.Value.ToString();
                }

                if (!string.IsNullOrEmpty(request.CustomerEmail))
                {
                    intentOptions.ReceiptEmail = request.CustomerEmail;
                }

                var intentService = new PaymentIntentService();
                var paymentIntent = await intentService.CreateAsync(intentOptions);

                return Ok(new PaymentResponse
                {
                    Success = true,
                    Message = "Paiement initié avec succès",
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Erreur Stripe: {ex.Message}"
                });
            }
        }

        // GET: api/Payment/status/{paymentIntentId}
        [HttpGet("status/{paymentIntentId}")]
        public async Task<ActionResult<PaymentResponse>> GetPaymentStatus(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                return Ok(new PaymentResponse
                {
                    Success = true,
                    PaymentIntentId = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    ChargeId = paymentIntent.LatestChargeId,
                    Message = $"Statut du paiement: {paymentIntent.Status}"
                });
            }
            catch (StripeException ex)
            {
                return NotFound(new PaymentResponse
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                });
            }
        }

        // POST: api/Payment/confirm/{paymentIntentId}
        [HttpPost("confirm/{paymentIntentId}")]
        public async Task<ActionResult<PaymentResponse>> ConfirmPayment(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.ConfirmAsync(paymentIntentId);

                return Ok(new PaymentResponse
                {
                    Success = paymentIntent.Status == "succeeded",
                    PaymentIntentId = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    ChargeId = paymentIntent.LatestChargeId,
                    Message = paymentIntent.Status == "succeeded" 
                        ? "Paiement confirmé avec succès" 
                        : $"Statut du paiement: {paymentIntent.Status}"
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                });
            }
        }

        // POST: api/Payment/refund/{paymentIntentId}
        [HttpPost("refund/{paymentIntentId}")]
        public async Task<ActionResult<PaymentResponse>> RefundPayment(string paymentIntentId)
        {
            try
            {
                var intentService = new PaymentIntentService();
                var paymentIntent = await intentService.GetAsync(paymentIntentId);

                if (string.IsNullOrEmpty(paymentIntent.LatestChargeId))
                {
                    return BadRequest(new PaymentResponse
                    {
                        Success = false,
                        Message = "Aucune charge à rembourser"
                    });
                }

                var refundOptions = new RefundCreateOptions
                {
                    Charge = paymentIntent.LatestChargeId
                };

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(refundOptions);

                return Ok(new PaymentResponse
                {
                    Success = refund.Status == "succeeded",
                    Message = refund.Status == "succeeded" 
                        ? "Remboursement effectué avec succès" 
                        : $"Statut du remboursement: {refund.Status}",
                    ChargeId = refund.ChargeId,
                    Status = refund.Status
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                });
            }
        }

        // GET: api/Payment/config
        [HttpGet("config")]
        public ActionResult<object> GetStripeConfig()
        {
            return Ok(new
            {
                PublishableKey = _configuration["Stripe:PublishableKey"]
            });
        }
    }
}

