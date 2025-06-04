using air_reservation.Data_Access_Layer;
using air_reservation.Models.Flight_Model_;
using air_reservation.Models.Passenger_Model_;
using Microsoft.EntityFrameworkCore;

namespace air_reservation.Repository.Flight_Repo
{
    public class FlightService : IFlightService
    {
        private readonly ApplicationDbContext _context;

        public FlightService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FlightDTO>> SearchFlightsAsync(FlightSearchDTO searchDto)
        {
            var query = _context.Flights.AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.Origin))
            {
                query = query.Where(f => f.Origin.Contains(searchDto.Origin));
            }

            if (!string.IsNullOrEmpty(searchDto.Destination))
            {
                query = query.Where(f => f.Destination.Contains(searchDto.Destination));
            }

            if (searchDto.DepartureDate.HasValue)
            {
                var date = searchDto.DepartureDate.Value.Date;
                query = query.Where(f => f.DepartureDateTime.Date == date);
            }

            // Filter by available seats based on preferred class
            if (searchDto.PreferredClass == SeatClass.Economy)
            {
                query = query.Where(f => f.AvailableEconomySeats >= searchDto.Passengers);
            }
            else
            {
                query = query.Where(f => f.AvailableBusinessSeats >= searchDto.Passengers);
            }

            var flights = await query.ToListAsync();

            return flights.Select(MapToFlightDTO).ToList();
        }

        public async Task<FlightDTO> GetFlightByIdAsync(int flightId)
        {
            var flight = await _context.Flights.FindAsync(flightId);
            return flight != null ? MapToFlightDTO(flight) : null;
        }

        public async Task<List<FlightDTO>> GetAllFlightsAsync()
        {
            var flights = await _context.Flights.ToListAsync();
            return flights.Select(MapToFlightDTO).ToList();
        }

        public async Task<FlightDTO> CreateFlightAsync(FlightDTO flightDto)
        {
            var flight = new Flight
            {
                FlightNumber = flightDto.FlightNumber,
                Airline = flightDto.Airline,
                Origin = flightDto.Origin,
                Destination = flightDto.Destination,
                DepartureDateTime = flightDto.DepartureDateTime,
                ArrivalDateTime = flightDto.ArrivalDateTime,
                TotalEconomySeats = flightDto.AvailableEconomySeats,
                TotalBusinessSeats = flightDto.AvailableBusinessSeats,
                AvailableEconomySeats = flightDto.AvailableEconomySeats,
                AvailableBusinessSeats = flightDto.AvailableBusinessSeats,
                EconomyPrice = flightDto.EconomyPrice,
                BusinessPrice = flightDto.BusinessPrice,
                Aircraft = flightDto.Aircraft,
                Status = flightDto.Status
            };

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return MapToFlightDTO(flight);
        }

        public async Task<FlightDTO> UpdateFlightAsync(int id, FlightDTO flightDto)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return null;

            flight.FlightNumber = flightDto.FlightNumber;
            flight.Airline = flightDto.Airline;
            flight.Origin = flightDto.Origin;
            flight.Destination = flightDto.Destination;
            flight.DepartureDateTime = flightDto.DepartureDateTime;
            flight.ArrivalDateTime = flightDto.ArrivalDateTime;
            flight.EconomyPrice = flightDto.EconomyPrice;
            flight.BusinessPrice = flightDto.BusinessPrice;
            flight.Aircraft = flightDto.Aircraft;
            flight.Status = flightDto.Status;

            await _context.SaveChangesAsync();
            return MapToFlightDTO(flight);
        }

        public async Task<bool> DeleteFlightAsync(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return false;

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return true;
        }

        private FlightDTO MapToFlightDTO(Flight flight)
        {
            return new FlightDTO
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                Airline = flight.Airline,
                Origin = flight.Origin,
                Destination = flight.Destination,
                DepartureDateTime = flight.DepartureDateTime,
                ArrivalDateTime = flight.ArrivalDateTime,
                AvailableEconomySeats = flight.AvailableEconomySeats,
                AvailableBusinessSeats = flight.AvailableBusinessSeats,
                EconomyPrice = flight.EconomyPrice,
                BusinessPrice = flight.BusinessPrice,
                Aircraft = flight.Aircraft,
                Status = flight.Status
            };
        }
    }
}
