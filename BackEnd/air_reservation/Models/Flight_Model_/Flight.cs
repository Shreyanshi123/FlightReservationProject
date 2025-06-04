using System.ComponentModel.DataAnnotations;
using air_reservation.Models.Reservation_Model_;

namespace air_reservation.Models.Flight_Model_
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        public string? FlightNumber { get; set; }

        [Required]
        public string? Airline { get; set; }

        [Required]
        public string? Origin { get; set; }

        [Required]
        public string? Destination { get; set; }

        public DateTime DepartureDateTime { get; set; }

        public DateTime ArrivalDateTime { get; set; }

        public int TotalEconomySeats { get; set; }

        public int TotalBusinessSeats { get; set; }

        public int AvailableEconomySeats { get; set; }

        public int AvailableBusinessSeats { get; set; }

        public decimal EconomyPrice { get; set; }

        public decimal BusinessPrice { get; set; }

        public string? Aircraft { get; set; }

        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    public enum FlightStatus
    {
        Scheduled,
        Delayed,
        Cancelled,
        Completed
    }
}

