using air_reservation.Models.Reservation_Model_;

namespace air_reservation.Repository.Reservation_Repo
{
    public interface IReservationService
    {
        Task<int> CleanupExpiredReservationsAsync();
        Task ProcessRefund(string transactionId, decimal refundAmount);
     

            Task<ReservationDTO> CreateReservationAsync(int userId, CreateReservationDTO createReservationDto);
        Task<List<ReservationDTO>> GetUserReservationsAsync(int userId);
        Task<ReservationDTO> GetReservationByIdAsync(int reservationId, int userId);
        Task<(bool, string, decimal)> CancelReservationAsync(int reservationId, int userId);
        Task<decimal> CalculateTotalAmountAsync(int flightId, List<PassengerDTO> passengers);

        Task<PaginatedReservationHistoryDTO> GetReservationHistoryAsync(int userId, ReservationHistoryFilterDTO filter);
        Task<List<ReservationHistoryDTO>> GetUpcomingJourneysAsync(int userId);
        Task<List<ReservationHistoryDTO>> GetPastJourneysAsync(int userId, int limit = 10);
        Task<ReservationHistoryDTO> GetReservationHistoryByIdAsync(int reservationId, int userId);
        Task<ReservationHistorySummaryDTO> GetReservationSummaryAsync(int userId);
        Task<List<ReservationHistoryDTO>> GetRecentBookingsAsync(int userId, int limit = 5);
        Task<bool> CanCancelReservationAsync(int reservationId, int userId);
        Task<bool> CanModifyReservationAsync(int reservationId, int userId);

        Task<List<PassengerDTO>> GetPassengersByReservationAsync(int reservationId);
        Task<bool> UpdatePassengerAsync(int passengerId, PassengerDTO updatedPassenger);
    
}
}
