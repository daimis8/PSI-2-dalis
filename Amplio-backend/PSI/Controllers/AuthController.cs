using Microsoft.AspNetCore.Mvc;
using PSI.Exceptions;
using PSI.Services;

namespace PSI.Controllers
{
	[ApiController]
	[Route("auth")]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _auth;

		public AuthController(AuthService auth)
		{
			_auth = auth;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] LoginRequest req)
		{
            try
            {
                var token = await _auth.Register(req.Username, req.Password);
                return Ok(new { token });
            }
            catch (UsernameAlreadyExistsException ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
            catch (InvalidPasswordException ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });  
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest req)
		{
            try
            {
                var token = await _auth.Login(req.Username, req.Password);
                return Ok(new { token });
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { message = ex.Message }); 
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }
	}

	public class LoginRequest
	{
		public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
	}
}
