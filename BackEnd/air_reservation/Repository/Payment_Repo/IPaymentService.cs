using air_reservation.Models.Reservation_Model_;

namespace air_reservation.Repository.Payment_Repo
{
    public interface IPaymentService
    {
        Task<PaymentDTO> ProcessPaymentAsync(ProcessPaymentDTO processPaymentDto);
        Task<PaymentDTO> GetPaymentByReservationIdAsync(int reservationId);
        Task<bool> RefundPaymentAsync(int paymentId);
    }
}
