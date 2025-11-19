using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddCors(options =>
//{
//	options.AddDefaultPolicy(
		
//		policy =>
//		{
//			policy.WithOrigins("http://localhost:5050")
//								.AllowAnyHeader()
//								.AllowAnyMethod();
//		});
		
	 
//});

// Tilføj Ocelot
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);

var key = builder.Configuration["Jwt:Key"]; // samme secret som i Auth service
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
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
			ClockSkew = TimeSpan.FromSeconds(60)
		};
		options.Events = new JwtBearerEvents
		{
			//OnMessageReceived = ctx =>
			//{
			//	if (ctx.Request.Headers.TryGetValue("Authorization", out var header))
			//	{
			//		var raw = header.ToString();
			//		Console.WriteLine($"[GATEWAY RAW HEADER TRUE] => {raw}");

			//		if (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			//		{
			//			ctx.Token = raw.Substring("Bearer ".Length).Trim();
			//			Console.WriteLine($"[GATEWAY CLEAN TOKEN] => {ctx.Token}");
			//		}
			//		else
			//		{
			//			ctx.Token = raw.Trim();
			//		}
			//	}
			//	return Task.CompletedTask;
			//},

			OnMessageReceived = context =>
			{
				Console.WriteLine("Authorization Header: " + context.Request.Headers["Authorization"]);
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = ctx =>
			{
				Console.WriteLine("[GATEWAY] JWT validation failed: " + ctx.Exception.Message);
				if (ctx.Exception.InnerException != null)
					Console.WriteLine("Inner: " + ctx.Exception.InnerException.Message);
				return Task.CompletedTask;
			},

			//OnAuthenticationFailed = ctx =>
			//{

			//	Console.WriteLine($"[GATEWAY] JWT validation failed: {ctx.Exception.Message}");

			//	return Task.CompletedTask;
			//},
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

//app.UseCors();

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
