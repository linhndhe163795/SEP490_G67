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

    [Fact]
    public async Task CreateDriverAsync_ValidDriver_ReturnsDriver()
    {
        // Arrange
        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = "dungnd",
            Name = "Nguyen Van Dung",
            NumberPhone = "0913530333",
            Password = "123456789",
            Avatar = "avatar.jpg",
            Dob = new DateTime(1990, 1, 1),
            License = "D",
            Status = true
        };

        // Mocking password hashing
       var expectedHashedPassword = "25f9e794323b453885f5181f1b624d0b";

        // Act
        var result = await _driverRepository.CreateDriverAsync(updateDriverDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dungnd", result.UserName);
        Assert.Equal("Nguyen Van Dung", result.Name);
        Assert.Equal("hashedPassword", result.Password);
        Assert.Equal("Active", result.StatusWork);
    }

    [Fact]
    public async Task CreateDriverAsync_UsernameExists_ThrowsException()
    {
        // Arrange
        var existingDriver = new Driver
        {
            UserName = "dungnd",
            Name = "Existing User"
        };

        // Tạo danh sách dữ liệu giả lập
        var drivers = new List<Driver> { existingDriver }.AsQueryable();

        // Mock DbSet<Driver>
        var mockDbSet = new Mock<DbSet<Driver>>();
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(drivers.Provider);
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(drivers.Expression);
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(drivers.ElementType);
        mockDbSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(drivers.GetEnumerator());

        // Gán Mock DbSet vào context
        _contextMock.Setup(ctx => ctx.Drivers).Returns(mockDbSet.Object);

        var updateDriverDto = new UpdateDriverDTO
        {
            UserName = "dungnd", // UserName đã tồn tại
            Name = "New User",
            NumberPhone = "0913530333",
            Password = "123456789",
            License = "D",
            Status = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _driverRepository.CreateDriverAsync(updateDriverDto));

        Assert.Equal("User Name is exist in system", exception.Message);
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
}
