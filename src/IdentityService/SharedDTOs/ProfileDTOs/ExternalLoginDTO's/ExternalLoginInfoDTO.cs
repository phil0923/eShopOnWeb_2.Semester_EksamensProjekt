namespace IdentityService.SharedDTOs.ProfileDTOs;

public class ExternalLoginInfoDTO
{
    public string LoginProvider { get; set; } = default!;
    public string ProviderKey { get; set; } = default!;
    public string? ProviderDisplayName { get; set; }
}
