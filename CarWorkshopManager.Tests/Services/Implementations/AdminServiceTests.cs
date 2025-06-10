using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Models.Identity;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections;
using System.Threading;

public class AdminServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        _adminService = new AdminService(_mockUserManager.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsListOfUserListItemViewModel()
    {
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "testuser1", FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "111-222-3333" },
            new ApplicationUser { Id = "2", UserName = "testuser2", FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "444-555-6666" }
        };
        var usersAsyncQueryable = new TestAsyncEnumerable<ApplicationUser>(users);
        _mockUserManager.Setup(um => um.Users).Returns(usersAsyncQueryable);
        _mockUserManager.Setup(um => um.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "1"))).ReturnsAsync(new List<string> { "Admin" });
        _mockUserManager.Setup(um => um.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "2"))).ReturnsAsync(new List<string> { "Mechanic" });
        var result = await _adminService.GetAllUsersAsync();
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(2, result.Count);
        Xunit.Assert.Contains(result, u => u.Username == "testuser1" && u.FullName == "John Doe" && u.Role == "Admin" && u.Email == "john.doe@example.com" && u.PhoneNumber == "111-222-3333");
        Xunit.Assert.Contains(result, u => u.Username == "testuser2" && u.FullName == "Jane Smith" && u.Role == "Mechanic" && u.Email == "jane.smith@example.com" && u.PhoneNumber == "444-555-6666");
    }

    [Fact]
    public async Task ChangeUserRoleAsync_ValidUserAndNewRole_ReturnsTrue()
    {
        var userId = "123";
        var newRole = "NewRole";
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "OldRole" });
        _mockUserManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddToRoleAsync(user, newRole)).ReturnsAsync(IdentityResult.Success);
        var result = await _adminService.ChangeUserRoleAsync(userId, newRole);
        Xunit.Assert.True(result);
        _mockUserManager.Verify(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, newRole), Times.Once);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_UserNotFound_ReturnsFalse()
    {
        var userId = "nonexistent";
        var newRole = "NewRole";
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);
        var result = await _adminService.ChangeUserRoleAsync(userId, newRole);
        Xunit.Assert.False(result);
        _mockUserManager.Verify(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _mockUserManager.Verify(um => um.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_RemoveRoleFails_ReturnsFalse()
    {
        var userId = "123";
        var newRole = "NewRole";
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "OldRole" });
        _mockUserManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));
        var result = await _adminService.ChangeUserRoleAsync(userId, newRole);
        Xunit.Assert.False(result);
        _mockUserManager.Verify(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_AddRoleFails_ReturnsFalse()
    {
        var userId = "123";
        var newRole = "NewRole";
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        _mockUserManager.Setup(um => um.AddToRoleAsync(user, newRole)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));
        var result = await _adminService.ChangeUserRoleAsync(userId, newRole);
        Xunit.Assert.False(result);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, newRole), Times.Once);
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner) { _inner = inner; }
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().First();
            var enumerType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(enumerType, expression)!;
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) { return new TestAsyncEnumerable<TElement>(expression); }
        public object? Execute(Expression expression) { return _inner.Execute(expression); }
        public TResult Execute<TResult>(Expression expression) { return _inner.Execute<TResult>(expression); }
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) { return new TestAsyncEnumerable<TResult>(expression); }
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) { return _inner.Execute<TResult>(expression); }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) { return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator()); }
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) { _inner = inner; }
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return new ValueTask(); }
        public ValueTask<bool> MoveNextAsync() { return new ValueTask<bool>(_inner.MoveNext()); }
    }
}
