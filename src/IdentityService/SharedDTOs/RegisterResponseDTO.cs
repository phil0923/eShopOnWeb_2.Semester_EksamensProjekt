using Azure.Core;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.SharedDTOs
{
	public class RegisterResponseDTO
	{
	
		public required string Token { get; set; }

		
		public required bool Success { get; set; }

	
		public required string Message { get; set; }
			

	}
}
