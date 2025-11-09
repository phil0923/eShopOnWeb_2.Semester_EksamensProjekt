using Ardalis.GuardClauses;
using IdentityService.Identity;
using IdentityService.Services.Interfaces;
using IdentityService.SharedDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using IdentityService.Services.Interfaces;

namespace IdentityService.Services
{
	public class EmailConfirmationService : IEmailConfirmationService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		 private readonly IEmailSender _emailSender;
		private readonly IConfiguration _config;

		public EmailConfirmationService(UserManager<ApplicationUser>userManager, IEmailSender emailSender,IConfiguration config)
		{
			_userManager = userManager;
			_emailSender = emailSender;
			_config = config;
		}

		public async Task<bool> SendEmailAsync(string email, ApplicationUser user)
		{

			if (user == null)
				return false;
			// Generate email confirmation link
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

		

			var monolithBaseUrl = _config["baseUrls:webBase"];
			var callbackUrl = $"{monolithBaseUrl}/Identity/Account/ConfirmEmail?userId={user.Id}&code={Uri.EscapeDataString(code)}";

			Guard.Against.Null(callbackUrl, nameof(callbackUrl));

			await _emailSender.SendEmailAsync(email, "Confirm your email",
				$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
			
			return true;

		}
	}
}
