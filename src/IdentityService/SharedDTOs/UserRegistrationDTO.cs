using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs
{
	public class UserRegistrationDTO
	{
		[Required]
		public required string Email { get; set; }

		[Required]
		public required string Password { get; set; }

	}
}
