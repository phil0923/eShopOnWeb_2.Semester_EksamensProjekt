using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs.ProfileDTOs;


public class ConfirmEmailDTO
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;
}

