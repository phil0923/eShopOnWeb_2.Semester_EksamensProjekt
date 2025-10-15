using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using IdentityService.Identity;
using IdentityService.SharedDTOs;

namespace IdentityService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		public AuthController(IConfiguration config,UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser>signInManager)
		{
			_config = config;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpPost("login")]
		 public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
		{
			// Simulate checking credentials
			var userDTO = new { Id = 1, Username = "alice", Role = "user", Password = "1234" };

			var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);

			if (user==null)
			{
				return Unauthorized("Invalid userName or password");	
			}


			var result = await _signInManager.CheckPasswordSignInAsync(user,userLoginDTO.Password,false);

			if (!result.Succeeded)
			{
				return Unauthorized("Invalid username or password");
			}

			var roles=await _userManager.GetRolesAsync(user);

		

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
			return Ok(new {token=tokenString});
		}


	}
}
