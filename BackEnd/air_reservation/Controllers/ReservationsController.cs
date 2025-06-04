//using air_reservation.Models.Reservation_Model_;
//using air_reservation.Repository.Reservation_Repo;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace air_reservation.Controllers
//{
//    [Route("api/[controller]/[action]")]
//    [ApiController]
//    public class ReservationsController : ControllerBase
//    {
//        private readonly IReservationService _reservationService;

//        public ReservationsController(IReservationService reservationService)
//        {
//            _reservationService = reservationService;
//        }

//        [HttpPost]
//        public async Task<ActionResult<ReservationDTO>> CreateReservation(int userId, [FromBody] CreateReservationDTO createReservationDto)
//        {
//            try
//            {
//                var reservation = await _reservationService.CreateReservationAsync(userId, createReservationDto);
//                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
//            }
//            catch (InvalidOperationException ex)
//            {
//                return BadRequest(new { message = ex.Message });
//            }
//        }

//        [HttpGet]

//        public async Task<ActionResult<List<ReservationDTO>>> GetUserReservations()
//        {
//            int userId = GetCurrentUserId();
//            var reservations = await _reservationService.GetUserReservationsAsync(userId);
//            return Ok(reservations);
//        }

//        [HttpGet("{id}")]
//        public async Task<ActionResult<ReservationDTO>> GetReservation(int id)
//        {
//            int userId = GetCurrentUserId();
//            var reservation = await _reservationService.GetReservationByIdAsync(id, userId);

//            if (reservation == null)
//                return NotFound();

//            return Ok(reservation);
//        }

//        [HttpPost("{id}/cancel")]
//        public async Task<ActionResult> CancelReservation(int id)
//        {
//            int userId = GetCurrentUserId();
//            var result = await _reservationService.CancelReservationAsync(id, userId);

//            if (!result)
//                return NotFound();

//            return Ok(new { message = "Reservation cancelled successfully" });
//        }

//        [HttpPost("calculate-amount")]
//        public async Task<ActionResult<decimal>> CalculateTotalAmount([FromBody] CreateReservationDTO createReservationDto)
//        {
//            try
//            {
//                var amount = await _reservationService.CalculateTotalAmountAsync(createReservationDto.FlightId, createReservationDto.Passengers);
//                return Ok(new { totalAmount = amount });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return BadRequest(new { message = ex.Message });
//            }
//        }

//        private int GetCurrentUserId()
//        {
//            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
//            Console.WriteLine(string.Join(", ", claims)); // Debugging step


//            var claim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
//            if (claim == null || string.IsNullOrEmpty(claim.Value))
//            {
//                throw new UnauthorizedAccessException("User ID claim is missing.");
//            }

//            return int.Parse(claim.Value);

//        }
//    }
//}

using air_reservation.Data_Access_Layer;
using air_reservation.Models.Reservation_Model_;
using air_reservation.Repository.Reservation_Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace air_reservation.Controllers

{

    [Route("api/[controller]/[action]")]

    [ApiController]

    public class ReservationsController : ControllerBase

    {

        private readonly IReservationService _reservationService;
        private readonly ApplicationDbContext _context;

        public ReservationsController(IReservationService reservationService, ApplicationDbContext context)

        {

            _reservationService = reservationService;
            _context = context;

        }


        [HttpGet("{reservationId}/passengers")]
        public async Task<ActionResult<List<PassengerDTO>>> GetPassengersByReservation(int reservationId)
        {
            var passengers = await _reservationService.GetPassengersByReservationAsync(reservationId);
            return passengers.Any() ? Ok(passengers) : NotFound(new { message = "No passengers found." });
        }


        [HttpDelete("CancelRoundTrip/{reservationId}/{returnReservationId}")]
        public async Task<IActionResult> CancelRoundTrip(int reservationId, int returnReservationId)
        {
            var outboundBooking = await _context.Reservations.FindAsync(reservationId);
            var returnBooking = await _context.Reservations.FindAsync(returnReservationId);

            if (outboundBooking == null || returnBooking == null)
                return NotFound("One or both reservations not found.");

            // ✅ Cancel both reservations
            outboundBooking.Status = ReservationStatus.Cancelled;

            returnBooking.Status = ReservationStatus.Cancelled;


        await _context.SaveChangesAsync();
            return Ok(new { message = "Roundtrip booking cancelled successfully!" });
        }


        [HttpPut("passengers/{passengerId}")]
        public async Task<ActionResult> UpdatePassenger(int passengerId, [FromBody] PassengerDTO updatedPassenger)
        {
            var success = await _reservationService.UpdatePassengerAsync(passengerId, updatedPassenger);
            return success ? Ok(new { message = "Passenger updated successfully." }) : NotFound(new { message = "Passenger not found." });
        }



        [HttpPost]
        [Authorize]

        public async Task<ActionResult<ReservationDTO>> CreateReservation([FromBody] CreateReservationDTO createReservationDto)

        {
            int userId = GetCurrentUserId();
            try

            {

                var reservation = await _reservationService.CreateReservationAsync(userId, createReservationDto);

                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);

            }

            catch (InvalidOperationException ex)

            {

                return BadRequest(new { message = ex.Message });

            }

        }

        [HttpGet]

        public async Task<ActionResult<List<ReservationDTO>>> GetUserReservations()

        {

            var userId = GetCurrentUserId();

            var reservations = await _reservationService.GetUserReservationsAsync(userId);

            return Ok(reservations);

        }

        [HttpGet("{id}")]

        public async Task<ActionResult<ReservationDTO>> GetReservation(int id)

        {

            var userId = GetCurrentUserId();

            var reservation = await _reservationService.GetReservationByIdAsync(id, userId);

            if (reservation == null)

                return NotFound();

            return Ok(reservation);

        }
        [HttpPut("CleanupExpired")]
        public async Task<IActionResult> CleanupExpiredReservations()
        {
            var expiredReservations = await _reservationService.CleanupExpiredReservationsAsync(); // Call service
            return Ok(new { updatedCount = expiredReservations });
        }


        [HttpDelete("{id}")]

        public async Task<ActionResult> CancelReservation(int id)

        {

            var userId = GetCurrentUserId();

            var (success, message, refundAmount) = await _reservationService.CancelReservationAsync(id, userId);

            if (!success)

                return NotFound();

            return Ok(new { message = "Reservation cancelled successfully" , refundAmount= refundAmount});

        }

        [HttpPost("calculate-amount")]

        public async Task<ActionResult<decimal>> CalculateTotalAmount([FromBody] CreateReservationDTO createReservationDto)

        {

            try

            {

                var amount = await _reservationService.CalculateTotalAmountAsync(createReservationDto.FlightId, createReservationDto.Passengers);

                return Ok(new { totalAmount = amount });

            }

            catch (InvalidOperationException ex)

            {

                return BadRequest(new { message = ex.Message });

            }

        }


        /// <summary>

        /// Get paginated reservation history with filters

        /// </summary>

        [HttpGet]

        public async Task<ActionResult<PaginatedReservationHistoryDTO>> GetReservationHistory([FromQuery] ReservationHistoryFilterDTO filter)

        {

            var userId = GetCurrentUserId();

            var result = await _reservationService.GetReservationHistoryAsync(userId, filter);

            return Ok(result);

        }

        /// <summary>

        /// Get upcoming journeys for the user

        /// </summary>

        [HttpGet("upcoming")]

        public async Task<ActionResult<List<ReservationHistoryDTO>>> GetUpcomingJourneys()

        {

            var userId = GetCurrentUserId();

            var upcomingJourneys = await _reservationService.GetUpcomingJourneysAsync(userId);

            return Ok(upcomingJourneys);

        }

        /// <summary>

        /// Get past journeys for the user

        /// </summary>

        [HttpGet("past")]

        public async Task<ActionResult<List<ReservationHistoryDTO>>> GetPastJourneys([FromQuery] int limit = 10)

        {

            var userId = GetCurrentUserId();

            var pastJourneys = await _reservationService.GetPastJourneysAsync(userId, limit);

            return Ok(pastJourneys);

        }

        /// <summary>

        /// Get detailed information about a specific reservation

        /// </summary>

        [HttpGet("{id}")]

        public async Task<ActionResult<ReservationHistoryDTO>> GetReservationHistory(int id)

        {

            var userId = GetCurrentUserId();

            var reservation = await _reservationService.GetReservationHistoryByIdAsync(id, userId);

            if (reservation == null)

                return NotFound(new { message = "Reservation not found" });

            return Ok(reservation);

        }

        /// <summary>

        /// Get reservation summary and statistics

        /// </summary>

        [HttpGet("summary")]

        public async Task<ActionResult<ReservationHistorySummaryDTO>> GetReservationSummary()

        {

            var userId = GetCurrentUserId();

            var summary = await _reservationService.GetReservationSummaryAsync(userId);

            return Ok(summary);

        }

        /// <summary>

        /// Get recent bookings

        /// </summary>

        [HttpGet("recent")]

        public async Task<ActionResult<List<ReservationHistoryDTO>>> GetRecentBookings([FromQuery] int limit = 5)

        {

            var userId = GetCurrentUserId();

            var recentBookings = await _reservationService.GetRecentBookingsAsync(userId, limit);

            return Ok(recentBookings);

        }

        /// <summary>

        /// Check if a reservation can be cancelled

        /// </summary>

        [HttpGet("{id}/can-cancel")]

        public async Task<ActionResult<bool>> CanCancelReservation(int id)

        {

            var userId = GetCurrentUserId();

            var canCancel = await _reservationService.CanCancelReservationAsync(id, userId);

            return Ok(new { canCancel });

        }

        /// <summary>

        /// Check if a reservation can be modified

        /// </summary>

        [HttpGet("{id}/can-modify")]

        public async Task<ActionResult<bool>> CanModifyReservation(int id)

        {

            var userId = GetCurrentUserId();

            var canModify = await _reservationService.CanModifyReservationAsync(id, userId);

            return Ok(new { canModify });

        }

        private int GetCurrentUserId()

        {

            return int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "0");

        }


    }

}

