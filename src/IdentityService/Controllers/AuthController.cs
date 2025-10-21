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
using IdentityService.Identity;

namespace IdentityService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger<AuthController> _logger;
		public AuthController(IConfiguration config, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ILogger<AuthController> logger)
		{
			_config = config;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
		{
			// Simulate checking credentials
			var userDTO = new { Id = 1, Username = "alice", Role = "user", Password = "1234" };

			var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);

			if (user == null)
			{
				return Unauthorized("Invalid userName or password");
			}


			var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDTO.Password, false);

			if (!result.Succeeded)
			{
				return Unauthorized("Invalid username or password");
			}

			var roles = await _userManager.GetRolesAsync(user);



			var claims = new List<Claim>()
			{

				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),

			 };


			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));

			}


			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"])),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return Ok(new { token = tokenString });
		}



		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userRegistrationDTO)
		{
			
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var user = new ApplicationUser
				{
					UserName = userRegistrationDTO.Email,
					Email = userRegistrationDTO.Email
				};

				var result = await _userManager.CreateAsync(user, userRegistrationDTO.Password);
				if (!result.Succeeded)
				{
					foreach (var error in result.Errors)
						ModelState.AddModelError(string.Empty, error.Description);
					return BadRequest(ModelState);
				}

				await _userManager.AddToRoleAsync(user, AppIdentityDbContextSeed.CUSTOMERS);

			_logger.LogInformation("User created a new account with password.");

				// Generate email confirmation link
				var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			var monolithBaseUrl = _config["baseUrls:webBase"];
			var callbackUrl = $"{monolithBaseUrl}/Account/ConfirmEmail?userId={user.Id}&code={Uri.EscapeDataString(code)}";
			
			Guard.Against.Null(callbackUrl, nameof(callbackUrl));

				await _emailSender.SendEmailAsync(userRegistrationDTO.Email, "Confirm your email",
					$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");



			var signInResult = await _signInManager.CheckPasswordSignInAsync(user, userRegistrationDTO.Password, false);

	
			var roles = await _userManager.GetRolesAsync(user);



			var claims = new List<Claim>()
			{

				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),

			 };


			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));

			}


			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"])),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return Ok(new { token = tokenString ,
				success = true,
				message = "Registration successful. Please confirm your email.",
			});


		
		}

	
	}
}
