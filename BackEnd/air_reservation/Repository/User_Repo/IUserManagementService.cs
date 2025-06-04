using air_reservation.Models.Users_Model_;

namespace air_reservation.Repository.User_Repo
{
    public interface IUserManagementService
    {
        Task<PagedResult<UserListDTO>> GetUsersAsync(UserSearchDTO searchDto);
        Task<UserDetailsDTO> GetUserByIdAsync(int userId);

        Task<UserDetailsDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDTO changePasswordDto);
        Task<List<UserListDTO>> GetRecentUsersAsync(int count = 10);
        Task<Dictionary<string, object>> GetUserStatisticsAsync();

        Task<UserDetailsDTO> UpdateUserProfileAsync(int userId, UpdateUserProfileDTO updateUserProfileDto);
    
}
}
