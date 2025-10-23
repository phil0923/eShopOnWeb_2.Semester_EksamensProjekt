using IdentityService.Identity;

namespace IdentityService.Services
{
	
		public class ServiceResult
		{
			public bool Success { get; set; }
			public string? Message { get; set; }
			public IEnumerable<string>? Errors { get; set; }

			public ApplicationUser? User { get; set; }

		public static ServiceResult Ok(string message = "", ApplicationUser? user = null) =>
				new() { Success = true, Message = message, User=user };


		public static ServiceResult Fail(IEnumerable<string> errors) =>
				new() { Success = false, Errors = errors };
		}
	
}
