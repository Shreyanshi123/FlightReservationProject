using air_reservation.Data_Access_Layer;
using air_reservation.Models.Registration_Model_;
using air_reservation.Models.Users_Model_;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace air_reservation.Repository.Login_Repo
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                // Log failed login attempt
                await LogLoginAttempt(loginDto.Email,null, false, null);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Log successful login
            await LogLoginAttempt(loginDto.Email,loginDto.Password, true, user.Id);

            var token = GenerateJwtToken(user.Id, user.Email, user.Role);

            return new AuthResponseDTO
            {
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var user = new User
            {
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = UserRole.User // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log registration
            await LogRegistration(registerDto, user.Id);

            var token = GenerateJwtToken(user.Id, user.Email, user.Role);

            return new AuthResponseDTO
            {
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public string GenerateJwtToken(int userId, string email, UserRole role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task LogLoginAttempt(string email,string password ,bool isSuccessful, int? userId)
        {
            var loginLog = new LoginModel
            {
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                IsSuccessful = isSuccessful,
                LoginTime = DateTime.UtcNow,
                IpAddress = "Unknown", // You can inject HttpContext to get real IP
                UserAgent = "Unknown"  // You can inject HttpContext to get real UserAgent
            };

            _context.LoginLogs.Add(loginLog);
            await _context.SaveChangesAsync();
        }

        private async Task LogRegistration(RegisterDTO registerDto, int userId)
        {
            var registrationLog = new RegisterModel
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Password = registerDto.Password,
                PhoneNumber = registerDto.PhoneNumber,
                Role = UserRole.User,
                RegistrationDate = DateTime.UtcNow,
                IpAddress = "Unknown", // You can inject HttpContext to get real IP
                VerificationToken = Guid.NewGuid().ToString()
            };

            _context.RegistrationLogs.Add(registrationLog);
            await _context.SaveChangesAsync();
        }
    }
}

