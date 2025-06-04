using air_reservation.Models.Flight_Model_;

namespace air_reservation.Repository.Flight_Repo
{
    public interface IFlightService
    {
        Task<List<FlightDTO>> SearchFlightsAsync(FlightSearchDTO searchDto);
        Task<FlightDTO> GetFlightByIdAsync(int flightId);
        Task<List<FlightDTO>> GetAllFlightsAsync();
        Task<FlightDTO> CreateFlightAsync(FlightDTO flightDto);
        Task<FlightDTO> UpdateFlightAsync(int id, FlightDTO flightDto);
        Task<bool> DeleteFlightAsync(int id);
    }
}
