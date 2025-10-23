using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdentityService
{
	
	public class NoOpEmailSender : IEmailSender
	{
		public Task SendEmailAsync(string to, string subject, string message)
		{
			// Does nothing, for demo/testing
			return Task.CompletedTask;
		}
	}

	
}
