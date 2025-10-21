using IdentityService.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore.Design;
using Swashbuckle.AspNetCore.Swagger;
using IdentityService;
using Microsoft.AspNetCore.Identity.UI.Services;
using NoOpEmailSender = IdentityService.NoOpEmailSender;

var builder = WebApplication.CreateBuilder(args);

// Database connection
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));
// Identity setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppIdentityDbContext>()
	.AddDefaultTokenProviders();



builder.Services.AddControllers();

builder.Services.AddScoped<IEmailSender, NoOpEmailSender>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers(); // enables attribute routing for controllers

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
	dbContext.Database.Migrate();
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



