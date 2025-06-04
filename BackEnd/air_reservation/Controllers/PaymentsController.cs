using air_reservation.Models.Payment_Model_;
using air_reservation.Models.Reservation_Model_;
using air_reservation.Repository.Payment_Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace air_reservation.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentDTO>> ProcessPayment([FromBody] ProcessPaymentDTO processPaymentDto)
        {
            try
            {
                var result = await _paymentService.ProcessPaymentAsync(processPaymentDto);
                if (result.Status == PaymentStatus.Failed)
                    return BadRequest("Payment failed. Please try again.");


                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reservation/{reservationId}")]
        public async Task<ActionResult<PaymentDTO>> GetPaymentByReservation(int reservationId)
        {
            var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpPost("{id}/refund")]
        public async Task<ActionResult> RefundPayment(int id)
        {
            var result = await _paymentService.RefundPaymentAsync(id);

            if (!result)
                return BadRequest(new { message = "Unable to process refund" });

            return Ok(new { message = "Refund processed successfully" });
        }
    }
}
