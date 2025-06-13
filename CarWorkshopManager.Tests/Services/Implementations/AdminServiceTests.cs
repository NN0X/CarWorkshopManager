using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using Microsoft.Extensions.Logging.Abstractions;

namespace CarWorkshopManager.Tests.Services.Implementations
{
    public class AdminServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            _adminService = new AdminService(_userManagerMock.Object, NullLogger<AdminService>.Instance); 
        }

        [Fact]
        public async Task ChangeUserRoleAsync_UserNotFound_ReturnsFalse()
        {
            _userManagerMock.Setup(u => u.FindByIdAsync("id")).ReturnsAsync((ApplicationUser)null);

            var result = await _adminService.ChangeUserRoleAsync("id", "NewRole");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_RemoveRolesFails_ReturnsFalse()
        {
            var user = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "OldRole" });
            _userManagerMock
                .Setup(u => u.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed());

            var result = await _adminService.ChangeUserRoleAsync("1", "NewRole");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_AddRoleFails_ReturnsFalse()
        {
            var user = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManagerMock
                .Setup(u => u.AddToRoleAsync(user, "NewRole"))
                .ReturnsAsync(IdentityResult.Failed());

            var result = await _adminService.ChangeUserRoleAsync("1", "NewRole");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_Succeeds_ReturnsTrue()
        {
            var user = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Old" });
            _userManagerMock
                .Setup(u => u.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(u => u.AddToRoleAsync(user, "NewRole"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _adminService.ChangeUserRoleAsync("1", "NewRole");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_UserNotFound_ReturnsFailedResult()
        {
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync((ApplicationUser)null);

            var result = await _adminService.DeleteUserAsync("1");

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("nie istnieje"));
        }

        [Fact]
        public async Task DeleteUserAsync_Succeeds_ReturnsSuccess()
        {
            var user = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _adminService.DeleteUserAsync("1");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            var user = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);

            var result = await _adminService.GetUserByIdAsync("1");

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task UpdateUserAsync_UserNotFound_ReturnsFailedResult()
        {
            var updated = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync((ApplicationUser)null);

            var result = await _adminService.UpdateUserAsync(updated);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("nie istnieje"));
        }

        [Fact]
        public async Task UpdateUserAsync_Succeeds_ReturnsSuccess()
        {
            var existing = new ApplicationUser
            {
                Id = "1", FirstName = "Old", LastName = "Name", Email = "old@example.com", PhoneNumber = "000"
            };
            
            var updated = new ApplicationUser
            {
                Id = "1", FirstName = "New", LastName = "Name", Email = "new@example.com", PhoneNumber = "111"
            };
            
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(existing);
            _userManagerMock.Setup(u => u.NormalizeEmail(updated.Email)).Returns(updated.Email.ToUpper());
            _userManagerMock
                .Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _adminService.UpdateUserAsync(updated);

            Assert.True(result.Succeeded);
            Assert.Equal("New", existing.FirstName);
            Assert.Equal("new@example.com", existing.Email);
            Assert.Equal("111", existing.PhoneNumber);
        }
    }
}
