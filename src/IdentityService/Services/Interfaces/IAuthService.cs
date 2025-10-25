using IdentityService.Identity;
using IdentityService.SharedDTOs;
using System.Threading.Tasks;

namespace IdentityService.Services.Interfaces
{
	public interface IAuthService
	{
		Task<ServiceResult> RegisterUserAsync(UserRegistrationDTO userRegistrationDTO, string password);

		Task<string> LoginUserAsync(string email, string password);

	}
}
