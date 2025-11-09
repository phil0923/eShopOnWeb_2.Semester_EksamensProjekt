using Ardalis.GuardClauses;
using Azure.Core;
using IdentityService.Identity;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.SharedDTOs;
using IdentityService.SharedDTOs;
using IdentityService.SharedDTOs.ProfileDTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
namespace IdentityService.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger<AccountController> _logger;
		private readonly IAuthService _authService;
		private readonly IEmailConfirmationService _emailConfirmationService;
		private readonly UrlEncoder _urlEncoder;

		private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
		public AccountController(IConfiguration config, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ILogger<AccountController> logger, IAuthService authService, IEmailConfirmationService emailConfirmationService, UrlEncoder urlEncoder)
		{
			_config = config;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_authService = authService;
			_emailConfirmationService = emailConfirmationService;
			_urlEncoder = urlEncoder;
		}


		[HttpGet("profile")]
		public async Task<ActionResult<UserProfileDTO>> GetProfile()
		{
			_logger.LogInformation("Auth Header: {Header}", Request.Headers["Authorization"].ToString());

			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			return new UserProfileDTO
			{
				Username = user.UserName,
				Email = user.Email,
				PhoneNumber = user.PhoneNumber,
				IsEmailConfirmed=user.EmailConfirmed
				
			};
		}



		[HttpPut("profile")]
		public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			user.Email = dto.Email;
			user.PhoneNumber = dto.PhoneNumber;
			await _userManager.UpdateAsync(user);

			return NoContent();
		}

		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound("User not found");

			var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
			if (!result.Succeeded) return BadRequest(result.Errors);

			return Ok();
		}

		[HttpPost("send-verification")]
		public async Task<IActionResult> SendEmailVerification(SendVerificationEmailDTO verificationEmailDTO)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
			
				return BadRequest();
			}

			var email = user.Email;
			if (verificationEmailDTO.Email != email)
			{
				var setEmailResult = await _userManager.SetEmailAsync(user, verificationEmailDTO.Email);
				if (!setEmailResult.Succeeded)
				{
				
					

					return BadRequest(setEmailResult.Errors);
				}
			}

			var phoneNumber = user.PhoneNumber;
			if (verificationEmailDTO.PhoneNumber != phoneNumber)
			{
				var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, verificationEmailDTO.PhoneNumber);
				if (!setPhoneResult.Succeeded)
				{
					return BadRequest(setPhoneResult.Errors);
				}
			}

			if (string.IsNullOrEmpty(email))
			{
				return BadRequest($"The email '{email}' is not a valid address.");
			}

			await _emailConfirmationService.SendEmailAsync(email, user);



			return Ok();

		}


		// ==============================
		// 2FA (Two-Factor Authentication)
		// ==============================

		[HttpGet("2fa/status")]
		public async Task<IActionResult> GetTwoFactorStatus()
		{
			// Returns current 2FA state (enabled, has authenticator, recovery codes left)

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return BadRequest($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var statusDTO = new TwoFactorStatusDTO
			{
				HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
				Is2faEnabled = user.TwoFactorEnabled,
				RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
			};

			return Ok(statusDTO);
		}

		[HttpPost("2fa/enable")]
		public async Task<IActionResult> EnableTwoFactor(EnableTwoFactorDTO dto)
		{
			// Enables 2FA for the user

			if (!ModelState.IsValid)
				return BadRequest(ModelState);


			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return NotFound($"User was not found");

			var isValid = await _userManager.VerifyTwoFactorTokenAsync(
				user, _userManager.Options.Tokens.AuthenticatorTokenProvider, dto.VerificationCode);

			if (!isValid)
				return BadRequest(new { error = "Verification code is invalid." });

			await _userManager.SetTwoFactorEnabledAsync(user, true);
			var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

			if(recoveryCodes==null)
			{
				return BadRequest("recoverycodes could not be generated for the user");
			}

			return Ok(new RecoveryCodesDTO()
			{
				RecoveryCodes = recoveryCodes.ToArray()
			});
		}

		[HttpPost("2fa/disable")]
		public async Task<IActionResult> DisableTwoFactor()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized();

			await _userManager.SetTwoFactorEnabledAsync(user, false);

			// Optionally log the action or return some info
			return Ok(new { message = "Two-factor authentication disabled." });
		}

		[HttpPost("2fa/reset-authenticator")]
		public async Task<IActionResult> ResetAuthenticator()
		{
			// Resets authenticator key
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized();

			await _userManager.ResetAuthenticatorKeyAsync(user);

			// Optionally log the action or return some info
			return Ok(new { message = "Authenticator key has been reset." });
		}

		[HttpGet("2fa/setup")]
		public async Task<IActionResult> GetTwoFactorSetupInfo()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
					?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound(new { error = $"User with ID '{userId}' not found." });

			var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
			if (string.IsNullOrEmpty(unformattedKey))
			{
				await _userManager.ResetAuthenticatorKeyAsync(user);
				unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
			}

			var formattedKey = FormatKey(unformattedKey!);
			var qrCodeUri = GenerateQrCodeUri(user.Email!, unformattedKey!);

			return Ok(new TwoFactorSetupDTO
			{
				SharedKey = formattedKey,
				AuthenticatorUri = qrCodeUri
			});
		}

		[HttpPost("2fa/generate-recovery-codes")]
		public async Task<IActionResult> GenerateRecoveryCodes()
		{
			// Creates new recovery codes

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return BadRequest($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			if (!user.TwoFactorEnabled)
			{
				return BadRequest($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
			}

			var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10) ?? new List<string>();

			return Ok(new RecoveryCodesDTO() { RecoveryCodes = recoveryCodes.ToArray() });
		}

		// ==============================
		// Password Management
		// ==============================

		[HttpGet("has-password")]
		public async Task<IActionResult> HasPassword()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized(new { message = "User not found." });

			var hasPassword = await _userManager.HasPasswordAsync(user);
			return Ok(new { hasPassword });
		}


		[HttpPost("set-password")]
		public async Task<IActionResult> SetPassword(SetPasswordDTO dto)
		{
			// Sets an initial password for external users
			// Validate model (similar to ModelState.IsValid)
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Get the user by their ID (you could also use token-based user identification here)
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound(new { message = "User not found." });
			}

			// Set the new password for the user
			var addPasswordResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
			if (!addPasswordResult.Succeeded)
			{
				return BadRequest(new { message = "Failed to set password.", errors = addPasswordResult.Errors });
			}

			// Optionally, sign in the user (if required in your API flow)
			// await _signInManager.SignInAsync(user, isPersistent: false);

			return Ok(new { message = "Password has been set successfully." });


		}

		// ==============================
		// Email Confirmation
		// ==============================

		[HttpPost("confirm-email")]
		public async Task<IActionResult> ConfirmEmail(ConfirmEmailDTO dto)
		{
			if (dto.UserId == null || dto.Token == null)
			{
				return BadRequest("No UserId or token was in the request to confirm email");
			}

			var user = await _userManager.FindByIdAsync(dto.UserId);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{dto.UserId}'.");
			}

			var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
			if (!result.Succeeded)
			{
				return BadRequest($"Error confirming email for user with ID '{dto.UserId}':");
			}

			return Ok(result);
		}

		// ==============================
		// External Logins (Optional)
		// ==============================

		[HttpGet("external-logins")]
		public async Task<ActionResult<ExternalLoginsDTO>> GetExternalLogins()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized();

			var currentLogins = await _userManager.GetLoginsAsync(user);
			var availableProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();

			var otherLogins = availableProviders
				.Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
				.Select(auth => new ExternalLoginProviderDTO
				{
					Name = auth.Name,
					DisplayName = auth.DisplayName
				})
				.ToList();

			var dto = new ExternalLoginsDTO
			{
				CurrentLogins = currentLogins.Select(l => new ExternalLoginInfoDTO
				{
					LoginProvider = l.LoginProvider,
					ProviderKey = l.ProviderKey,
					ProviderDisplayName = l.ProviderDisplayName
				}).ToList(),
				OtherLogins = otherLogins,
				CanRemove = await _userManager.HasPasswordAsync(user) || currentLogins.Count > 1
			};

			return Ok(dto);
		}


		[HttpGet("link-login")]
		public IActionResult LinkLogin([FromQuery] string provider)
		{
			// Redirect back to identity microservice for the callback
			var redirectUrl = Url.Action(nameof(LinkLoginCallback), "Account", null, Request.Scheme);

			// Configure OAuth properties
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

			if (properties == null)
				return BadRequest("External auth props could not be configured");


			// Start external login challenge (redirects user to Google, etc.)
			return new ChallengeResult(provider, properties);
		}

		[AllowAnonymous]
		[HttpGet("link-login-callback")]
		public async Task<IActionResult> LinkLoginCallback()
		{
			// The external login provider just redirected the user here.
			// Retrieve the user and external login info.
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized();

			var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
			if (info == null)
			{
				var errorUrl = $"{_config["baseUrls:webBase"]}/Manage/LinkLoginCallback?status=error&reason=noinfo";
				return Redirect(errorUrl);
			}

			var result = await _userManager.AddLoginAsync(user, info);
			if (!result.Succeeded)
			{
				var errorUrl = $"{_config["baseUrls:webBase"]}/Manage/LinkLoginCallback?status=error&reason=addloginfailed";
				return Redirect(errorUrl);
			}

			// Clear any temporary cookies used by the external provider
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			// Redirect the user back to the monolith UI, which will handle displaying the success message
			var successUrl = $"{_config["baseUrls:webBase"]}/Manage/LinkLoginCallback?status=success";
			return Redirect(successUrl);
		}


		[HttpPost("remove-login")]
		public async Task<IActionResult> RemoveLogin([FromBody] RemoveLoginDTO dto)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized();

			var result = await _userManager.RemoveLoginAsync(user, dto.LoginProvider, dto.ProviderKey);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return Ok(new { message = "External login removed successfully." });
		}
		// ==============================
		// Account Utilities
		// ==============================

		[HttpGet("recovery-codes")]
		public async Task<ActionResult<RecoveryCodesStatusDTO>> GetRecoveryCodes()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized(new { message = "User not found." });

			if (!user.TwoFactorEnabled)
				return BadRequest(new { message = "2FA is not enabled for this user." });

			var count = await _userManager.CountRecoveryCodesAsync(user);

			var dto = new RecoveryCodesStatusDTO
			{
				Message = "Recovery codes are not retrievable for security reasons.",
				RecoveryCodesLeft = count,

			};

			return Ok(dto);
		}

		[HttpGet("recovery-codes/warning")]
		public async Task<ActionResult<RecoveryCodesWarningDTO>> GetGenerateRecoveryCodesWarning()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Unauthorized(new { message = "User not found." });

			if (!user.TwoFactorEnabled)
				return BadRequest(new { message = "2FA must be enabled before generating recovery codes." });

			var remaining = await _userManager.CountRecoveryCodesAsync(user);

			var dto = new RecoveryCodesWarningDTO
			{
				Message = "Generating new recovery codes will invalidate any remaining old codes.",
				RecoveryCodesLeft = remaining
			};

			return Ok(dto);
		}


		private string GenerateQrCodeUri(string email, string unformattedKey)
		{
			return string.Format(
				AuthenticatorUriFormat,
				_urlEncoder.Encode("YourAppName"),
				_urlEncoder.Encode(email),
				unformattedKey);
		}
		private static string FormatKey(string unformattedKey)
		{
			var result = new StringBuilder();
			int currentPosition = 0;
			while (currentPosition + 4 < unformattedKey.Length)
			{
				result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
				currentPosition += 4;
			}
			if (currentPosition < unformattedKey.Length)
			{
				result.Append(unformattedKey.Substring(currentPosition));
			}
			return result.ToString().ToLowerInvariant();
		}
	};

}
