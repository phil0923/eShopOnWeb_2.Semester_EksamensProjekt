using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs.ProfileDTOs;


public class ChangePasswordDTO
{
    [Required]
    public string OldPassword { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}

