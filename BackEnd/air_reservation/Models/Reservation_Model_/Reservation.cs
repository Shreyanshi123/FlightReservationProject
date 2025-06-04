using air_reservation.Models.Flight_Model_;
using air_reservation.Models.Passenger_Model_;
using air_reservation.Models.Payment_Model_;
using air_reservation.Models.Users_Model_;

namespace air_reservation.Models.Reservation_Model_
{
    public class Reservation
    {
        public int Id { get; set; }

        public string? BookingReference { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public decimal TotalAmount { get; set; }

        public List<Passenger> Passengers { get; set; } = new List<Passenger>();

        public Payment? Payment { get; set; }

        // ✅ New expiration timestamp
        public DateTime? ExpiresAt { get; set; }

    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Refunded
    }
}

