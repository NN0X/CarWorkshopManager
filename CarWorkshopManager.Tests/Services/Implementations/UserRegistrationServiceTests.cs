using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;

namespace CarWorkshopManager.Tests.Services.Implementations;
public class UserRegistrationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUsernameGeneratorService> _mockUsernameGenerator;
    private readonly UserRegistrationService _userRegistrationService;

    public UserRegistrationServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        _mockUsernameGenerator = new Mock<IUsernameGeneratorService>();
        _userRegistrationService = new UserRegistrationService(_mockUserManager.Object, _mockUsernameGenerator.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_SuccessfulRegistration_ReturnsSuccessAndUser()
    {
        var firstName = "Test";
        var lastName = "User";
        var email = "test@example.com";
        var phoneNumber = "123-456-7890";
        var role = "Customer";
        var generatedUsername = "tesuse1";
        var passwordResetToken = "testtoken";

        _mockUsernameGenerator.Setup(ug => ug.GenerateUsernameAsync(firstName, lastName))
                              .ReturnsAsync(generatedUsername);

        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
                        .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync(passwordResetToken);

        var (result, user, token) = await _userRegistrationService.RegisterUserAsync(firstName, lastName, email, phoneNumber, role);

        Xunit.Assert.True(result.Succeeded);
        Xunit.Assert.NotNull(user);
        Xunit.Assert.Equal(generatedUsername, user.UserName);
        Xunit.Assert.Equal(email, user.Email);
        Xunit.Assert.Equal(passwordResetToken, token);
        _mockUserManager.Verify(um => um.CreateAsync(It.Is<ApplicationUser>(u => u.UserName == generatedUsername && u.Email == email)), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, role), Times.Once);
        _mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_CreateUserFails_ReturnsFailureAndNullUser()
    {
        var firstName = "Test";
        var lastName = "User";
        var email = "test@example.com";
        var phoneNumber = "123-456-7890";
        var role = "Customer";
        var generatedUsername = "tesuse1";

        _mockUsernameGenerator.Setup(ug => ug.GenerateUsernameAsync(firstName, lastName))
                              .ReturnsAsync(generatedUsername);

        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Create failed" }));

        var (result, user, token) = await _userRegistrationService.RegisterUserAsync(firstName, lastName, email, phoneNumber, role);

        Xunit.Assert.False(result.Succeeded);
        Xunit.Assert.NotNull(result.Errors.First());
        Xunit.Assert.Null(user);
        Xunit.Assert.Null(token);
        _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_AddToRoleFails_ReturnsFailureAndUser()
    {
        var firstName = "Test";
        var lastName = "User";
        var email = "test@example.com";
        var phoneNumber = "123-456-7890";
        var role = "Customer";
        var generatedUsername = "tesuse1";

        _mockUsernameGenerator.Setup(ug => ug.GenerateUsernameAsync(firstName, lastName))
                              .ReturnsAsync(generatedUsername);

        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role add failed" }));

        var (result, user, token) = await _userRegistrationService.RegisterUserAsync(firstName, lastName, email, phoneNumber, role);

        Xunit.Assert.False(result.Succeeded);
        Xunit.Assert.NotNull(result.Errors.First());
        Xunit.Assert.NotNull(user); // User should be created even if role assignment fails
        Xunit.Assert.Null(token);
        _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, role), Times.Once);
        _mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}
