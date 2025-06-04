using air_reservation.Models.Flight_Model_;
using air_reservation.Repository.Flight_Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace air_reservation.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<FlightDTO>>> SearchFlights([FromQuery] FlightSearchDTO searchDto)
        {
            var flights = await _flightService.SearchFlightsAsync(searchDto);
            return Ok(flights);
        }

        [HttpGet]
        public async Task<ActionResult<List<FlightDTO>>> GetAllFlights()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            return Ok(flights);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightDTO>> GetFlight(int id)
        {
            var flight = await _flightService.GetFlightByIdAsync(id);
            if (flight == null)
                return NotFound();

            return Ok(flight);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can create flights
        public async Task<ActionResult<FlightDTO>> CreateFlight([FromBody] FlightDTO flightDto)
        {
            var createdFlight = await _flightService.CreateFlightAsync(flightDto);
            return CreatedAtAction(nameof(GetFlight), new { id = createdFlight.Id }, createdFlight);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can update flights
        public async Task<ActionResult<FlightDTO>> UpdateFlight(int id, [FromBody] FlightDTO flightDto)
        {
            var updatedFlight = await _flightService.UpdateFlightAsync(id, flightDto);
            if (updatedFlight == null)
                return NotFound();

            return Ok(updatedFlight);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete flights
        public async Task<ActionResult> DeleteFlight(int id)
        {
            var result = await _flightService.DeleteFlightAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
