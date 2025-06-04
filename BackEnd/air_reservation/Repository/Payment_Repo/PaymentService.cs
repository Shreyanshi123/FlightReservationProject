using air_reservation.Data_Access_Layer;
using air_reservation.Models.Passenger_Model_;
using air_reservation.Models.Payment_Model_;
using air_reservation.Models.Reservation_Model_;
using Microsoft.EntityFrameworkCore;

namespace air_reservation.Repository.Payment_Repo
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDTO> ProcessPaymentAsync(ProcessPaymentDTO processPaymentDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Flight)
                    .Include(r => r.Passengers)
                    .FirstOrDefaultAsync(r => r.Id == processPaymentDto.ReservationId);

                if (reservation == null)
                    throw new InvalidOperationException("Reservation not found");
                
                var existingPayment = await _context.Payments
           .FirstOrDefaultAsync(p => p.ReservationId == reservation.Id);


                //if (reservation.Status != ReservationStatus.Pending)
                //    throw new InvalidOperationException("Reservation is not in pending status");

                // Simulate payment processing
                var paymentResult = await SimulatePaymentGateway(processPaymentDto);
                if (existingPayment != null)
                {
                    if (existingPayment.Status == PaymentStatus.Failed)
                    {
                        // ✅ Update the failed payment
                        existingPayment.PaymentMethod = processPaymentDto.PaymentMethod;
                        existingPayment.Status = paymentResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
                        existingPayment.TransactionId = paymentResult.TransactionId;
                        existingPayment.PaymentGatewayResponse = paymentResult.Response;
                        existingPayment.PaymentDate = DateTime.UtcNow;
                    }
                    else
                    {
                        throw new InvalidOperationException("Payment already completed. Cannot pay twice.");
                    }
                }
                else
                {
                    // Create payment record
                    var payment = new Payment
                    {
                        ReservationId = reservation.Id,
                        Amount = reservation.TotalAmount,
                        PaymentMethod = processPaymentDto.PaymentMethod,
                        Status = paymentResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed,
                        TransactionId = paymentResult.TransactionId,
                        PaymentGatewayResponse = paymentResult.Response
                    };

                    _context.Payments.Add(payment);
                }
                if (paymentResult.Success)
                {
                    // Update reservation status
                    reservation.Status = ReservationStatus.Confirmed;

                    // Deduct seats from flight availability
                    var economyPassengers = reservation.Passengers.Count(p => p.SeatClass == SeatClass.Economy);
                    var businessPassengers = reservation.Passengers.Count(p => p.SeatClass == SeatClass.Business);

                    reservation.Flight.AvailableEconomySeats -= economyPassengers;
                    reservation.Flight.AvailableBusinessSeats -= businessPassengers;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new PaymentDTO
                {
                    Id = existingPayment?.Id ?? reservation.Id,
                    Amount = reservation.TotalAmount,
                    PaymentMethod = processPaymentDto.PaymentMethod,
                    Status = paymentResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed,
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = paymentResult.TransactionId

                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaymentDTO> GetPaymentByReservationIdAsync(int reservationId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null) return null;

            return new PaymentDTO
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                TransactionId = payment.TransactionId
            };
        }

        public async Task<bool> RefundPaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || payment.Status != PaymentStatus.Completed)
                return false;

            // Simulate refund processing
            payment.Status = PaymentStatus.Refunded;
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<PaymentResult> SimulatePaymentGateway(ProcessPaymentDTO paymentDto)
        {
            // Simulate payment processing delay
            await Task.Delay(1000);

            // Simulate success/failure (90% success rate)
            var random = new Random();
            var success = random.Next(1, 11) <= 9;

            return new PaymentResult
            {
                Success = success,
                TransactionId = "TXN" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + random.Next(1000, 9999),
                Response = success ? "Payment processed successfully" : "Payment failed - insufficient funds"
            };
        }

        private class PaymentResult
        {
            public bool Success { get; set; }
            public string TransactionId { get; set; }
            public string Response { get; set; }
        }
    }
}
