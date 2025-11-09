using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Tilføj Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
Console.WriteLine($"[GATEWAY] Key length: {"RH8xbRvSTPfeyXXFN+INbOnsyknoziH6UcsdiOLqqCo=".Length}");
var key = "MySuperSecureJwtSigningKey1234567890!"; // samme secret som i Auth service
var keyBytes = Encoding.UTF8.GetBytes(key); ; // decode to 32 real bytesConsole.WriteLine($"[IDENTITY] Key length: {keyBytes.Length}");
Console.WriteLine($"[IDENTITY] First bytes: {string.Join(",", keyBytes.Take(10))}");
builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = "Bearer";
		options.DefaultChallengeScheme = "Bearer";
	})
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
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
			ClockSkew = TimeSpan.Zero
		};
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = ctx =>
			{
				if (ctx.Request.Headers.TryGetValue("Authorization", out var header))
				{
					var raw = header.ToString();
					Console.WriteLine($"[GATEWAY RAW HEADER TRUE] => {raw}");

					if (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
					{
						ctx.Token = raw.Substring("Bearer ".Length).Trim();
						Console.WriteLine($"[GATEWAY CLEAN TOKEN] => {ctx.Token}");
					}
					else
					{
						ctx.Token = raw.Trim();
					}
				}
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = ctx =>
			{
				Console.WriteLine($"[GATEWAY] JWT validation failed: {ctx.Exception.Message}");
				return Task.CompletedTask;
			},
			OnTokenValidated = ctx =>
			{
				var claims = ctx.Principal?.Claims
					.Select(c => $"{c.Type}: {c.Value}")
					.ToList();

				Console.WriteLine("[GATEWAY] JWT successfully validated.");
				Console.WriteLine(string.Join("\n", claims ?? []));
				return Task.CompletedTask;
			}
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
