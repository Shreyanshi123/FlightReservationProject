using System.ComponentModel.DataAnnotations;

namespace air_reservation.Models.Users_Model_
{
    public class LoginModel
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public bool IsSuccessful { get; set; }
    }
}
