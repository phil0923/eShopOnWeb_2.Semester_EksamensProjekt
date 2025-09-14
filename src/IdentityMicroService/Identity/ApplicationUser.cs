using Microsoft.AspNetCore.Identity;

namespace IdentityMicroService.Identity;

public class ApplicationUser : IdentityUser
{
   public string FullName { get; set; } = string.Empty;
   

}
