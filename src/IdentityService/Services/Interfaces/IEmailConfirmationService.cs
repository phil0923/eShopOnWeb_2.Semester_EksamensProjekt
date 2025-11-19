using IdentityService.Identity;
using IdentityService.SharedDTOs;

namespace IdentityService.Services.Interfaces
{
	public interface IEmailConfirmationService
	{
		Task<bool> SendEmailAsync(string email, ApplicationUser user);
	}
}
