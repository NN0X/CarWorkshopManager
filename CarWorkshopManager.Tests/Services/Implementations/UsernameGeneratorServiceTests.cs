using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Models.Identity;

namespace CarWorkshopManager.Tests.Services.Implementations;
public class UsernameGeneratorServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly UsernameGeneratorService _usernameGeneratorService;

    public UsernameGeneratorServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        _usernameGeneratorService = new UsernameGeneratorService(_mockUserManager.Object,
            NullLogger<UsernameGeneratorService>.Instance);
    }

    [Fact]
    public async Task GenerateUsernameAsync_UniqueName_ReturnsCorrectUsername()
    {
        var firstName = "John";
        var lastName = "Doe";
        var expectedUsername = "johdoe1";

        _mockUserManager.Setup(um => um.FindByNameAsync(expectedUsername))
                        .ReturnsAsync((ApplicationUser)null!);

        var result = await _usernameGeneratorService.GenerateUsernameAsync(firstName, lastName);

        Xunit.Assert.Equal(expectedUsername, result);
    }

    [Fact]
    public async Task GenerateUsernameAsync_ExistingName_ReturnsUniqueUsernameWithSuffix()
    {
        var firstName = "Jane";
        var lastName = "Smith";
        var existingUsername1 = "jansmi1";
        var existingUsername2 = "jansmi2";
        var expectedUsername = "jansmi3";

        _mockUserManager.SetupSequence(um => um.FindByNameAsync(It.IsAny<string>()))
                        .ReturnsAsync(new ApplicationUser { UserName = existingUsername1 }) // jansmi1 exists
                        .ReturnsAsync(new ApplicationUser { UserName = existingUsername2 }) // jansmi2 exists
                        .ReturnsAsync((ApplicationUser)null!);                              // jansmi3 does not exist

        var result = await _usernameGeneratorService.GenerateUsernameAsync(firstName, lastName);

        Xunit.Assert.Equal(expectedUsername, result);
        _mockUserManager.Verify(um => um.FindByNameAsync("jansmi1"), Times.Once);
        _mockUserManager.Verify(um => um.FindByNameAsync("jansmi2"), Times.Once);
        _mockUserManager.Verify(um => um.FindByNameAsync("jansmi3"), Times.Once);
    }

    [Fact]
    public async Task GenerateUsernameAsync_ShortNames_GeneratesCorrectly()
    {
        var firstName = "Al";
        var lastName = "Li";
        var expectedUsername = "alli1";

        _mockUserManager.Setup(um => um.FindByNameAsync(expectedUsername))
                        .ReturnsAsync((ApplicationUser)null!);

        var result = await _usernameGeneratorService.GenerateUsernameAsync(firstName, lastName);

        Xunit.Assert.Equal(expectedUsername, result);
    }
}
