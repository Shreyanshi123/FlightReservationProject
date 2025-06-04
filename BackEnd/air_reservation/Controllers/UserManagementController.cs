using System.Security.Claims;
using air_reservation.Models.Users_Model_;
using air_reservation.Repository.User_Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace air_reservation.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        /// <summary>
        /// Get paginated list of users with search and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserListDTO>>> GetUsers([FromQuery] UserSearchDTO searchDto)
        {
            var result = await _userManagementService.GetUsersAsync(searchDto);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed information about a specific user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailsDTO>> GetUser(int id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>


        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDetailsDTO>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                var user = await _userManagementService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userManagementService.DeleteUserAsync(id);
                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Change a user's password
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
        {
            var result = await _userManagementService.ChangeUserPasswordAsync(id, changePasswordDto);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Get recently registered users
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<List<UserListDTO>>> GetRecentUsers([FromQuery] int count = 10)
        {
            if (count <= 0 || count > 100)
                count = 10;

            var users = await _userManagementService.GetRecentUsersAsync(count);
            return Ok(users);
        }

        /// <summary>
        /// Get user statistics for dashboard
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, object>>> GetUserStatistics()
        {
            var statistics = await _userManagementService.GetUserStatisticsAsync();
            return Ok(statistics);
        }

        [HttpGet]
        public async Task<ActionResult<UserDetailsDTO>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Invalid user token" });
            }

            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Update current user's profile details (excluding role)
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<UserDetailsDTO>> UpdateMyProfile([FromBody] UpdateUserProfileDTO updateUserProfileDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Invalid user token" });
            }

            try
            {
                var user = await _userManagementService.UpdateUserProfileAsync(userId, updateUserProfileDto);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Change current user's password
        /// </summary>
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Invalid user token" });
            }

            var result = await _userManagementService.ChangeUserPasswordAsync(userId, changePasswordDto);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "Password changed successfully" });
        }
    }
}
