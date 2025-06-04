using System.ComponentModel.DataAnnotations;
using air_reservation.Models.Reservation_Model_;

namespace air_reservation.Models.Users_Model_
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    public enum UserRole
    {
        User,
        Admin
    }
}

