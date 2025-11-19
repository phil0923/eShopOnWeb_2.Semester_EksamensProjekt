namespace IdentityService.SharedDTOs.ProfileDTOs;
public class RecoveryCodesStatusDTO
{
    public int RecoveryCodesLeft { get; set; }
    public string Message { get; set; } = string.Empty;
}
