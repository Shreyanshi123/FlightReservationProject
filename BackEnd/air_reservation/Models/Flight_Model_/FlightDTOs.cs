using air_reservation.Models.Passenger_Model_;

namespace air_reservation.Models.Flight_Model_
{
    public class FlightDTOs
    {
    }
    public class FlightSearchDTO
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime? DepartureDate { get; set; }
        public int Passengers { get; set; } = 1;
        public SeatClass PreferredClass { get; set; } = SeatClass.Economy;
    }

    public class FlightDTO
    {
        public int Id { get; set; }
        public string? FlightNumber { get; set; }
        public string? Airline { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public int AvailableEconomySeats { get; set; }
        public int AvailableBusinessSeats { get; set; }
        public decimal EconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }
        public string? Aircraft { get; set; }
        public FlightStatus Status { get; set; }
    }
}
