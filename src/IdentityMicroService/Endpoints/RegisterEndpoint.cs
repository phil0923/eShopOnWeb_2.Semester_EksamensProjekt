 
using Microsoft.AspNetCore.Identity;   // for UserManager & IdentityRole
using IdentityMicroService.DTOs;
using IdentityMicroService.Identity;
using IdentityMicroService;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMicroService
{
    public class RegisterEndpoint:IEndpoint<IResult>
    {
      

        public void AddRoute(IEndpointRouteBuilder app)
        {
            // IProductService can also be a repository or something else.
            app.MapPost("/register", async (
                [FromBody] RegisterRequest_DTO request,
                [FromServices] UserManager<ApplicationUser> userManager) =>
             {
                 // Create a new user entity from the request
                var user = new ApplicationUser
                {
                UserName = request.Username,
                Email = request.Email,
                };

             // CreateAsync will insert into AspNetUsers (via IdentityDbContext)
                var result = await userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    // Assign default role, e.g., "Customer"
                    await userManager.AddToRoleAsync(user, BlazorShared.Authorization.Constants.Roles.CUSTOMERS);
                    return Results.Ok(new { message = "User registered successfully" });
                }
                  
                  await userManager.AddToRoleAsync(user, BlazorShared.Authorization.Constants.Roles.CUSTOMERS);

                // If there are validation errors (weak password, duplicate email, etc.)
                return Results.BadRequest(result.Errors);
        
            });

      
        }
    }
}