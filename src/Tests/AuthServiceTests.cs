using Castle.Core.Logging;
using FluentAssertions;
using IdentityService.Identity;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;

namespace Tests;


public class AuthServiceTests
{
	private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
	private readonly Mock<ITokenService> _tokenServiceMock;
	private readonly Mock<ILogger<AuthService>> _loggerMock;
	private readonly Mock<IHttpContextAccessor> _contextAccessor;
	private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsFactory;
	private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
	private readonly Mock<IConfiguration> _configMock;
	private readonly AuthService _authService;


	public AuthServiceTests()
	{
		var store = new Mock<IUserStore<ApplicationUser>>();
		_configMock = new Mock<IConfiguration>();
		_userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, // IOptions<IdentityOptions>
	null, // IPasswordHasher<ApplicationUser>
	null,
	null,
	null, // ILookupNormalizer
	null, // IdentityErrorDescriber
	null, // IServiceProvider
	null);  // ILogger<UserManager<ApplicationUser>>);

		_tokenServiceMock = new Mock<ITokenService>();
		_loggerMock = new Mock<ILogger<AuthService>>();
		_contextAccessor = new Mock<IHttpContextAccessor>();
		_claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
		_signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
			_userManagerMock.Object,
			_contextAccessor.Object,
			_claimsFactory.Object,
			null, // IOptions<IdentityOptions>
			new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
			null, // IAuthenticationSchemeProvider
			new Mock<IUserConfirmation<ApplicationUser>>().Object
		);
		_authService = new AuthService(_userManagerMock.Object, _tokenServiceMock.Object, _signInManagerMock.Object, _loggerMock.Object);

	}

	[Fact]
	public async Task LoginUserAsync_ShouldReturnValidToken_WhenMailAndPassWordIsCorrect()
	{
		//Arrange
		string emailToCheck = "hello@live.dk";
		string passwordToCheck = "password";
		string tokenToReturn = "validToken";
		_userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser() { UserName = emailToCheck, Email = emailToCheck });
		_signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);
		_tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>())).ReturnsAsync(tokenToReturn);



		//Act

		var result = await _authService.LoginUserAsync(emailToCheck, passwordToCheck);


		//Assert
		result.Should().Be(tokenToReturn);

	}
}
