using Ardalis.GuardClauses;
using IdentityService.Identity;
using IdentityService.SharedDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using IdentityService.Services;
using IdentityService.Services.Interfaces;

namespace IdentityService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly ILogger<AuthController> _logger;
		private readonly IAuthService _authService;
		private readonly IEmailConfirmationService _emailConfirmationService;
		public AuthController(ILogger<AuthController> logger, IAuthService authService, IEmailConfirmationService emailConfirmationService)
		{
			_logger = logger;
			_authService = authService;
			_emailConfirmationService = emailConfirmationService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
		{

			var tokenString= await _authService.LoginUserAsync(userLoginDTO.Email, userLoginDTO.Password);
			
			_logger.LogInformation($"Generated token: {tokenString}");

			if (string.IsNullOrEmpty(tokenString))
				return Unauthorized("Invalid email or password");

			return Ok(new LoginResponseDTO { Token = tokenString, IsLockedOut=false,RequiresTwoFactor=false,Succeeded=true});
		}



		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userRegistrationDTO)
		{
			
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

			ServiceResult result = await _authService.RegisterUserAsync(userRegistrationDTO, userRegistrationDTO.Password);

			if (!result.Success)
			{
				if (result.Errors != null)
				{
					foreach (var error in result.Errors)
						ModelState.AddModelError(string.Empty, error);
				}
				return BadRequest(ModelState);
			}

			string? tokenString = null;

			if (result.User != null && userRegistrationDTO!=null)
			{
				await _emailConfirmationService.SendEmailAsync(userRegistrationDTO.Email, result.User);
				
				tokenString=await _authService.LoginUserAsync(userRegistrationDTO.Email, userRegistrationDTO.Password);
			}

	
			if (tokenString == null)
				return Unauthorized("Invalid email or password");

			return Ok(new RegisterResponseDTO { Token = tokenString,
				Success = true,
				Message = "Registration successful. Please confirm your email.",
			});


		
		}

	
	}
}
