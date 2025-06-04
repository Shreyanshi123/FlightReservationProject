using air_reservation.Models.Users_Model_;
using System.ComponentModel.DataAnnotations;

namespace air_reservation.Models.Registration_Model_
{
    public class RegisterModel
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public string? IpAddress { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public string? VerificationToken { get; set; }
    }
}

