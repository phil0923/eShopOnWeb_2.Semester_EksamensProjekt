namespace IdentityService.SharedDTOs.ProfileDTOs;

public class RecoveryCodesWarningDTO
{
    public string Message { get; set; } = string.Empty;
    public int RecoveryCodesLeft { get; set; }
}
