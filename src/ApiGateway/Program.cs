using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Tilføj Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var key = Encoding.ASCII.GetBytes("RH8xbRvSTPfeyXXFN+INbOnsyknoziH6UcsdiOLqqCo="); // samme secret som i Auth service

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "IdentityService",
        ValidAudience = "eShopClient",
        IssuerSigningKey = new SymmetricSecurityKey(key)
      };
    });

builder.Services.AddOcelot();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware til at sætte X-Client-Id headeren fra JWT claim
app.Use(async (context, next) =>
{
  if (context.User.Identity?.IsAuthenticated == true)
  {
    // Typisk claim i JWT der indeholder bruger-id
    var userId = context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("user_id")?.Value;
    if (!string.IsNullOrEmpty(userId))
    {
      context.Request.Headers["X-Client-Id"] = userId;
    }
  }

  await next.Invoke();
});

await app.UseOcelot();

app.Run();
