using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace IdentityService.Identity
{
	public class AppIdentityDbContextSeed
	{
		public const string ADMINISTRATORS = "Administrators";
		public const string CUSTOMERS = "Customers";
		public const string AUTH_KEY = "AuthKeyOfDoomThatMustBeAMinimumNumberOfBytes";

		// TODO: Don't use this in production
		public const string DEFAULT_PASSWORD = "Pass@word1";

		// TODO: Change this to an environment variable
		public const string JWT_SECRET_KEY = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";

		public static async Task SeedAsync(AppIdentityDbContext identityDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{


			string[] roles = {
		  ADMINISTRATORS,
		  CUSTOMERS
		};

			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}
			var defaultUserEmail = "demouser@microsoft.com";
			var defaultUser = await userManager.FindByEmailAsync(defaultUserEmail);
			if (defaultUser == null)
			{
				defaultUser = new ApplicationUser { UserName = defaultUserEmail, Email = defaultUserEmail };
				await userManager.CreateAsync(defaultUser, DEFAULT_PASSWORD);
				await userManager.AddToRoleAsync(defaultUser, CUSTOMERS);
			}


			string adminUserName = "admin@microsoft.com";
			var adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName };
			await userManager.CreateAsync(adminUser,DEFAULT_PASSWORD);
			adminUser = await userManager.FindByNameAsync(adminUserName);
			if (adminUser != null)
			{
				await userManager.AddToRoleAsync(adminUser,ADMINISTRATORS);
			}
		}
	}
}
