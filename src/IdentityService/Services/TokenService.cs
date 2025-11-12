using IdentityService.Identity;
using IdentityService.Services.Interfaces;
using IdentityService.SharedDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Unicode;

namespace IdentityService.Services
{
	public class TokenService : ITokenService
	{

		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _config;
	
		public TokenService(UserManager<ApplicationUser> userManager, IConfiguration config)
		{
			_userManager = userManager;
			_config = config;
			
		}

		public async Task<string> GenerateToken(ApplicationUser user)
		{
			

			var roles = await _userManager.GetRolesAsync(user);



			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
				new Claim(ClaimTypes.Email, user.Email ?? string.Empty)

			 };


			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));

			}

			var jwtKey = _config["Jwt:Key"]
	?? throw new InvalidOperationException("JWT Key is missing in configuration.");


			var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

			var key = new SymmetricSecurityKey(keyBytes);
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"])),
				signingCredentials: creds
				
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

			return tokenString;
;
		}
	}
}
