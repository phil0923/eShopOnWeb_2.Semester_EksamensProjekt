using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs.ProfileDTOs;

public class SetPasswordDTO
{

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
