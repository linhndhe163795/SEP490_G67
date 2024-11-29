using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MyAPI.Models;
using MyAPI.Helper;
using MyAPI.Repositories.Impls;
using MyAPI.DTOs.AccountDTOs;

public class AccountRepositoryTests
{
    private readonly DbContextOptions<SEP490_G67Context> _options;
    private readonly SEP490_G67Context _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<GetInforFromToken> _inforFromTokenMock;
    private readonly AccountRepository _accountRepository;

    public AccountRepositoryTests()
    {
        // In-Memory Database
        _options = new DbContextOptionsBuilder<SEP490_G67Context>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new SEP490_G67Context(_options);

        // Mocks
        _mapperMock = new Mock<IMapper>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _inforFromTokenMock = new Mock<GetInforFromToken>();

        // AccountRepository instance
        _accountRepository = new AccountRepository(
            _context,
            _mapperMock.Object,
            _httpContextAccessorMock.Object,
            _inforFromTokenMock.Object
        );
    }

    [Fact]
    public async Task DeteleAccount_AccountExists_ReturnsTrueAndUpdatesStatus()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FullName = "John Doe",
            Email = "johndoe@example.com",
            NumberPhone = "123456789",
            Password = "password123",
            Username = "johndoe",
            Status = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountRepository.DeteleAccount(1);

        // Assert
        Assert.True(result);
        var updatedUser = await _context.Users.FindAsync(1);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.Status); // Status should be false
    }

    [Fact]
    public async Task DeteleAccount_AccountDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _accountRepository.DeteleAccount(99);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public async Task GetDetailsUser_UserExists_ReturnsMappedAccountListDTO()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users);
        var user = new User
        {
            Id = 1,
            FullName = "John Doe",
            Email = "johndoe@example.com",
            Username = "johndoe",
            Password = "password123", // Giả định
            NumberPhone = "123456789" // Giả định
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var expectedDto = new AccountListDTO
        {
            Id = 1,
            FullName = "John Doe",
            Email = "johndoe@example.com",
            Username = "johndoe"
        };

        _mapperMock.Setup(m => m.Map<AccountListDTO>(user)).Returns(expectedDto);

        // Act
        var result = await _accountRepository.GetDetailsUser(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.FullName, result.FullName);
        Assert.Equal(expectedDto.Email, result.Email);
        Assert.Equal(expectedDto.Username, result.Username);
    }

    [Fact]
    public async Task GetDetailsUser_UserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _accountRepository.GetDetailsUser(999); // ID không tồn tại

        // Assert
        Assert.Null(result);
    }
    [Fact]
    public async Task GetListAccount_ReturnsAccountListDTOs()
    {
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Username = "johndoe",
                Password = "password123",
                NumberPhone = "123456789"
            },
            new User
            {
                Id = 2,
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                Username = "janesmith",
                Password = "password123",
                NumberPhone = "987654321"
            }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var expectedDto = new List<AccountListDTO>
        {
            new AccountListDTO { Id = 1, FullName = "John Doe", Email = "john.doe@example.com", Username = "johndoe" },
            new AccountListDTO { Id = 2, FullName = "Jane Smith", Email = "jane.smith@example.com", Username = "janesmith" }
        };

        _mapperMock.Setup(m => m.Map<List<AccountListDTO>>(users)).Returns(expectedDto);

        var result = await _accountRepository.GetListAccount();

        Assert.NotNull(result);
        Assert.Equal(expectedDto.Count, result.Count);
        Assert.Equal(expectedDto[0].FullName, result[0].FullName);
        Assert.Equal(expectedDto[1].Email, result[1].Email);
    }

    [Fact]
    public async Task UpdateRoleOfAccount_ValidUserAndRole_UpdatesRoleSuccessfully()
    {
        var user = new User
        {
            Id = 1,
            FullName = "John Doe",
            Email = "john.doe@example.com",
            Username = "johndoe",
            Password = "password123",
            NumberPhone = "123456789",
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = 1, RoleId = 1 }
            }
        };

        var role = new Role { Id = 2, RoleName = "Admin" };

        _context.Users.Add(user);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var token = "ValidToken";
        _httpContextAccessorMock
            .Setup(c => c.HttpContext.Request.Headers["Authorization"])
            .Returns($"Bearer {token}");

        _inforFromTokenMock
            .Setup(t => t.GetIdInHeader(token))
            .Returns(99);

        var result = await _accountRepository.UpdateRoleOfAccount(1, 2);

        Assert.True(result);

        var updatedUserRole = _context.UserRoles.SingleOrDefault(ur => ur.UserId == 1);
        Assert.NotNull(updatedUserRole);
        Assert.Equal(2, updatedUserRole.RoleId);

        var updatedUser = _context.Users.SingleOrDefault(u => u.Id == 1);
        Assert.NotNull(updatedUser);
        Assert.Equal(99, updatedUser.UpdateBy);
    }


}
