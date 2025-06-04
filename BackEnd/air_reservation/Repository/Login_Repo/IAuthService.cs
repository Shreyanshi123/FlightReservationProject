using air_reservation.Models.Users_Model_;

namespace air_reservation.Repository.Login_Repo
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);
        string GenerateJwtToken(int userId, string email, UserRole role);
    }
}
