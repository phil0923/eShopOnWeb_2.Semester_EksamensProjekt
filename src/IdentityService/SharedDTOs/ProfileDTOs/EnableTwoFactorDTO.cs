using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs.ProfileDTOs;

public class EnableTwoFactorDTO
{
    [Required]
    public string VerificationCode { get; set; } = null!;
}
