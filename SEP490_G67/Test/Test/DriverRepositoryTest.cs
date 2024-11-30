using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Helper;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;


public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
    {
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(source.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());

        return mockDbSet;
    }
}

public class DriverRepositoryTests
{
    private readonly Mock<SEP490_G67Context> _contextMock;
    private readonly Mock<ITypeOfDriverRepository> _typeOfDriverRepositoryMock;
    private readonly Mock<SendMail> _sendMailMock;
    private readonly Mock<HashPassword> _hashPasswordMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly DriverRepository _driverRepository;

    public DriverRepositoryTests()
    {
        // Mock the database context
        var options = new DbContextOptionsBuilder<SEP490_G67Context>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var context = new SEP490_G67Context(options);
        _contextMock = new Mock<SEP490_G67Context>(options);

        // Mock other dependencies
        _typeOfDriverRepositoryMock = new Mock<ITypeOfDriverRepository>();
        _sendMailMock = new Mock<SendMail>();
        _hashPasswordMock = new Mock<HashPassword>();
        _mapperMock = new Mock<IMapper>();

        // Initialize DriverRepository with mocked dependencies
        _driverRepository = new DriverRepository(
            context,
            _typeOfDriverRepositoryMock.Object,
            _sendMailMock.Object,
            _hashPasswordMock.Object,
            _mapperMock.Object
        );
    }
    private string ComputeHash(string input)
    {
        using (var md5 = MD5.Create())
        {
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    [Fact]
    public async Task CreateDriverAsync_NullPassword_ThrowsException()
    {
        // Arrange
        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = "dungnd",
            Name = "Nguyen Van Dung",
            NumberPhone = "0913530333",
            Password = null,
            License = "D",
            Status = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _driverRepository.CreateDriverAsync(updateDriverDto));
        Assert.Contains("Password cannot be null", exception.Message);
    }

    [Fact]
    public async Task CreateDriverAsync_NullUserName_ThrowsException()
    {
        // Arrange
        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = null,
            Name = "Nguyen Van Dung",
            NumberPhone = "0913530333",
            Password = "123456789",
            License = "D",
            Status = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _driverRepository.CreateDriverAsync(updateDriverDto));
        Assert.Contains("UserName cannot be null", exception.Message);
    }

    [Fact]
    public async Task CreateDriverAsync_NullNumberPhone_ThrowsException()
    {
        // Arrange
        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = "dungnd",
            Name = "Nguyen Van Dung",
            NumberPhone = null,
            Password = "123456789",
            License = "D",
            Status = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _driverRepository.CreateDriverAsync(updateDriverDto));
        Assert.Contains("NumberPhone cannot be null", exception.Message);
    }
    [Fact]
    public async Task UpdateDriverAsync_ValidDriver_ReturnsUpdatedDriver()
    {
        // Arrange
        var existingDriver = new Driver
        {
            Id = 1,
            UserName = "dungnd",
            Name = "Nguyen Van Dung",
            NumberPhone = "0913530333",
            Password = "25f9e794323b453885f5181f1b624d0b",
            License = "D",
            Status = true
        };

        _contextMock.Setup(ctx => ctx.Drivers.FindAsync(1)).ReturnsAsync(existingDriver);

        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = "newuser",
            Name = "Updated Name",
            NumberPhone = "0912123456",
            Password = "newpassword",
            License = "B",
            Status = true
        };
        // Act
        var result = await _driverRepository.UpdateDriverAsync(1, updateDriverDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser", updateDriverDto.UserName);
        Assert.Equal("Updated Name", updateDriverDto.Name);
        Assert.Equal("0912123456", updateDriverDto.NumberPhone);
        Assert.Equal("hashed_newpassword", updateDriverDto.Password);
        Assert.Equal("B", updateDriverDto.License);
        Assert.True(result.Status);
    }
    [Fact]
    public async Task UpdateDriverAsync_UsernameExists_ThrowsException()
    {
        // Arrange
        var existingDriver = new Driver
        {
            Id = 1,
            UserName = "dungnd",
            Name = "Nguyen Van Dung"
        };

        var otherDriver = new Driver
        {
            Id = 2,
            UserName = "newuser"
        };

        var drivers = new List<Driver> { existingDriver }.AsQueryable();
        var mockDbSet = new Mock<DbSet<Driver>>();
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(drivers.Provider);
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(drivers.Expression);
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(drivers.ElementType);
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(drivers.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Drivers).Returns(mockDbSet.Object);

        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = "newuser", // Username đã tồn tại
            Name = "Updated Name",
            NumberPhone = "0912123456",
            Password = "newpassword",
            License = "B",
            Status = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _driverRepository.UpdateDriverAsync(1, updateDriverDto));

        Assert.Equal("User Name already exists in the system", exception.Message);
    }
}
