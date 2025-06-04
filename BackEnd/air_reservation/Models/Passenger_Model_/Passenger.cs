using air_reservation.Models.Reservation_Model_;
using System.ComponentModel.DataAnnotations;

namespace air_reservation.Models.Passenger_Model_
{
    public class Passenger
    {
        public int Id { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        public int Age { get; set; }

        public Gender Gender { get; set; }

        public SeatClass SeatClass { get; set; }

        public string? SeatNumber { get; set; }

        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum SeatClass
    {
        Economy,
        Business
    }
}

