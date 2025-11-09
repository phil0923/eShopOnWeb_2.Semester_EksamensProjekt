using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs.ProfileDTOs;

public class SendVerificationEmailDTO
{

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
	public string? PhoneNumber { get; set; }

}
