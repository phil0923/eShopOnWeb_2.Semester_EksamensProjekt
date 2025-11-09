using IdentityService;
using IdentityService.Identity;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System.Security.Claims;
using System.Text;
using NoOpEmailSender = IdentityService.NoOpEmailSender;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"JWT Config => Issuer={builder.Configuration["Jwt:Issuer"]}, Audience={builder.Configuration["Jwt:Audience"]}");
// Database connection
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));
// Identity setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppIdentityDbContext>()
	.AddDefaultTokenProviders();


builder.Services.AddControllers();

builder.Services.AddScoped<IEmailSender, NoOpEmailSender>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtKey = builder.Configuration["Jwt:Key"]
	?? throw new InvalidOperationException("JWT Key is missing in configuration.");


var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
Console.WriteLine($"[IDENTITY] Key length: {keyBytes.Length}");
Console.WriteLine($"[IDENTITY] First bytes: {string.Join(",", keyBytes.Take(10))}");

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = "Bearer";
	options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			keyBytes),
		NameClaimType = ClaimTypes.NameIdentifier,
		RoleClaimType = ClaimTypes.Role
	};
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // enables attribute routing for controllers

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

	try
	{
		app.Logger.LogInformation("Checking database status...");

		// Use EF's database creator service directly for clarity
		var databaseCreator = db.Database.GetService<IRelationalDatabaseCreator>();
		// Try to connect directly first
		if (!db.Database.CanConnect())
		{
			if (!databaseCreator.Exists())
			{
				app.Logger.LogInformation("Database not found. Creating...");
				databaseCreator.Create();
			}
		}
		app.Logger.LogInformation("Applying any pending migrations...");
		db.Database.Migrate();

		app.Logger.LogInformation("Database is ready.");
	}
	catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1801)
	{
		// Database already exists — harmless if another instance created it
		app.Logger.LogWarning("Database already exists. Skipping creation.");
	}
	catch (Exception ex)
	{
		app.Logger.LogError(ex, "Error while initializing or migrating the database.");
	}

}


using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var context = services.GetRequiredService<AppIdentityDbContext>();
	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

	await AppIdentityDbContextSeed.SeedAsync(context, userManager, roleManager);
}


// Enable Swagger middleware
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.Run();



