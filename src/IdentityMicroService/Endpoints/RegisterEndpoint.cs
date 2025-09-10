 
namespace IdentityMicroService
public class RegisterEndpoint:IEndpoint<IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterEndpoint(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

  public void AddRoute(IEndpointRouteBuilder app)
  {
      // IProductService can also be a repository or something else.
      app.MapGet("/RegisterUser", async (RegisterRequest request) =>
      {
                 // Create a new user entity from the request
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                Name = request.FullName // your custom property
            };

            // CreateAsync will insert into AspNetUsers (via IdentityDbContext)
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                return Results.Ok(new { message = "User registered successfully" });
            }

            // If there are validation errors (weak password, duplicate email, etc.)
            return Results.BadRequest(result.Errors);
        
      });

      
  }
}