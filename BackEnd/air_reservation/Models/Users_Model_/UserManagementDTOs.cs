using System.ComponentModel.DataAnnotations;

namespace air_reservation.Models.Users_Model_
{
    public class UpdateUserDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }

    public class UserDetailsDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserListDTO
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReservationCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserSearchDTO
    {
        public string? SearchTerm { get; set; }
        public UserRole? Role { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
    public class UpdateUserProfileDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }
    }


}
