using IdentityService.Identity;
using IdentityService.Services.Interfaces;
using IdentityService.SharedDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		
		private readonly ITokenService _tokenService;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<AuthService> _logger;
		public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager, ILogger<AuthService> logger)
		{
			_userManager = userManager;
			_tokenService = tokenService;
			_signInManager = signInManager;
			_logger = logger;
		}

	

		public async Task<string?> LoginUserAsync(string email, string password)
		{
			ApplicationUser? user = await _userManager.FindByEmailAsync(email);

			if (user==null)
			{
				return null; 
			}

			var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

			if (!result.Succeeded)
			{
				return null;
			}

			string token = await _tokenService.GenerateToken(user);

			return token;
		}

		public async Task<ServiceResult> RegisterUserAsync(UserRegistrationDTO userRegistrationDTO, string password)
		{
			
			

			var newUser = new ApplicationUser
			{
				UserName = userRegistrationDTO.Email,
				Email = userRegistrationDTO.Email
			};

			var result = await _userManager.CreateAsync(newUser, userRegistrationDTO.Password);
		
			if (!result.Succeeded)
				return ServiceResult.Fail(result.Errors.Select(e => e.Description));
			

			await _userManager.AddToRoleAsync(newUser, AppIdentityDbContextSeed.CUSTOMERS);



			_logger.LogInformation("User created a new account with password.");

			return ServiceResult.Ok("User created a new account with password.",newUser) 
			;

		}
	}
}
