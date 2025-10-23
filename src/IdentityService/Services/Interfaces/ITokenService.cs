using IdentityService.Identity;

namespace IdentityService.Services.Interfaces
{
	public interface ITokenService
	{
		Task<string> GenerateToken(ApplicationUser user);

	}
}
