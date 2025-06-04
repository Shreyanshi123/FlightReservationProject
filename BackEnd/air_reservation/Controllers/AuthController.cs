using air_reservation.Models.Users_Model_;
using air_reservation.Repository.Login_Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace air_reservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ✅ Function to Verify CAPTCHA
        private async Task<bool> VerifyCaptchaAsync(string captchaToken)
        {
            if (string.IsNullOrEmpty(captchaToken))
                return false;

            using (var httpClient = new HttpClient())
            {
                var secretKey = "6LdEhFUrAAAAAPoB6zu9XRvYCoS2FNVJ6SUSVmxs"; // ✅ Replace with actual reCAPTCHA secret key
                var response = await httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaToken}");

                var jsonResponse = JObject.Parse(response);
                return jsonResponse.Value<bool>("success"); // ✅ Parse "success" from JSON response
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var isCaptchaValid = await VerifyCaptchaAsync(loginDto.CaptchaToken);
                Console.WriteLine($"🚀 Received CAPTCHA Token: {loginDto.CaptchaToken}");
                if (!isCaptchaValid)
                {
                    return BadRequest(new { message = "CAPTCHA verification failed!" });
                }


                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterDTO registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
