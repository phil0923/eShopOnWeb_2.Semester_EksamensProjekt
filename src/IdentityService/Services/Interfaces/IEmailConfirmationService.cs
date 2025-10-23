using IdentityService.Identity;
using IdentityService.SharedDTOs;

namespace IdentityService.Services.Interfaces
{
	public interface IEmailConfirmationService
	{
		Task<bool> SendEmail(string email, ApplicationUser user);
	}
}
