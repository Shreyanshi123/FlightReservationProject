using air_reservation.Models.Flight_Model_;
using air_reservation.Models.Passenger_Model_;
using air_reservation.Models.Payment_Model_;
using System.ComponentModel.DataAnnotations;

namespace air_reservation.Models.Reservation_Model_
{
    
    public class CreateReservationDTO
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public List<PassengerDTO>? Passengers { get; set; }
    }

    public class PassengerDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Range(1, 120)]
        public int Age { get; set; }

        public Gender Gender { get; set; }

        public SeatClass SeatClass { get; set; }
    }

    public class ReservationDTO
    {
        public int Id { get; set; }
        public string? BookingReference { get; set; }
        public FlightDTO? Flight { get; set; }
        public DateTime BookingDate { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PassengerDTO>? Passengers { get; set; }
        public PaymentDTO? Payment { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class PaymentDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
    }

    public class ProcessPaymentDTO
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        // Payment gateway specific fields
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
    }

    public class ReservationHistoryDTO
    {
        public int Id { get; set; }
        public string? BookingReference { get; set; }
        public FlightHistoryDTO?  Flight { get; set; }
        public DateTime BookingDate { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PassengerHistoryDTO>? Passengers { get; set; }
        public PaymentHistoryDTO? Payment { get; set; }
        public JourneyType JourneyType { get; set; } // Past, Upcoming, Current
        public int DaysUntilDeparture { get; set; }
        public bool CanCancel { get; set; }
        public bool CanModify { get; set; }
    }

    public class FlightHistoryDTO
    {
        public int Id { get; set; }
        public string? FlightNumber { get; set; }
        public string? Airline { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public decimal EconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }
        public string? Aircraft { get; set; }
        public FlightStatus Status { get; set; }
        public string Duration => CalculateFlightDuration();

        private string CalculateFlightDuration()
        {
            var duration = ArrivalDateTime - DepartureDateTime;
            return $"{duration.Hours}h {duration.Minutes}m";
        }
    }

    public class PassengerHistoryDTO
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public SeatClass SeatClass { get; set; }
        public string? SeatNumber { get; set; }
        public string? TicketNumber { get; set; }
    }

    public class PaymentHistoryDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string PaymentMethodDisplay => GetPaymentMethodDisplay();

        private string GetPaymentMethodDisplay()
        {
            return PaymentMethod switch
            {
                PaymentMethod.CreditCard => "Credit Card",
                PaymentMethod.DebitCard => "Debit Card",
                PaymentMethod.PayPal => "PayPal",
                PaymentMethod.BankTransfer => "Bank Transfer",
                _ => PaymentMethod.ToString()
            };
        }
    }

    public class ReservationHistoryFilterDTO
    {
        public JourneyType? JourneyType { get; set; }
        public ReservationStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Airline { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "BookingDate"; // BookingDate, DepartureDate, Amount
        public bool SortDescending { get; set; } = true;
    }

    public class ReservationHistorySummaryDTO
    {
        public int TotalBookings { get; set; }
        public int UpcomingJourneys { get; set; }
        public int CompletedJourneys { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalAmountSpent { get; set; }
        public decimal AmountThisYear { get; set; }
        public string? MostFrequentDestination { get; set; }
        public string? PreferredAirline { get; set; }
        public List<MonthlyBookingStatDTO>?   MonthlyStats { get; set; }
    }

    public class MonthlyBookingStatDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? MonthName { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaginatedReservationHistoryDTO
    {
        public List<ReservationHistoryDTO>? Reservations { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public enum JourneyType
    {
        Past,
        Upcoming,
        Current
    }
}
