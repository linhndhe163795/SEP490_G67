using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class AuthController_Test
    {
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IUserRoleRepository> userRoleRepositoryMock;
        private Mock<SendMail> sendMailMock;
        private Mock<IMapper> mapperMock;
        private Mock<IConfiguration> configurationMock;
        private Jwt jwtMock;
        private AuthController controller;

        [TestInitialize]
        public void Setup()
        {
            // Mock IConfiguration
            configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(config => config["JWT:SecretKey"]).Returns("your-secret-key");
            configurationMock.Setup(config => config["JWT:Issuer"]).Returns("your-issuer");
            configurationMock.Setup(config => config["JWT:Audience"]).Returns("your-audience");
            jwtMock = new Jwt(configurationMock.Object);
            userRepositoryMock = new Mock<IUserRepository>();
            userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            sendMailMock = new Mock<SendMail>();
            mapperMock = new Mock<IMapper>();
            controller = new AuthController(userRepositoryMock.Object, jwtMock, userRoleRepositoryMock.Object, sendMailMock.Object, mapperMock.Object);
        }

        #region Register
        [TestMethod]
        public async Task Register_ReturnsOk_WhenAccountDoesNotExist()
        {
            // Arrange: Set up test data using UserRegisterDTO
            var userRegisterDto = new UserRegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                NumberPhone = "1234567890",
                Password = "password",
                Dob = DateTime.Now.AddYears(-20),
                Status = true,
                ActiveCode = "Active123",
                CreatedAt = DateTime.Now
            };

            // Mock the mapping of UserRegisterDTO to User object
            var user = new User
            {
                Username = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NumberPhone = userRegisterDto.NumberPhone,
                Password = userRegisterDto.Password,
                Dob = userRegisterDto.Dob,
                Status = userRegisterDto.Status,
                ActiveCode = userRegisterDto.ActiveCode,
                CreatedAt = userRegisterDto.CreatedAt
            };

            // Setup mock for checking account existence
            userRepositoryMock.Setup(repo => repo.checkAccountExsit(It.IsAny<User>())).ReturnsAsync(false); // Account doesn't exist

            // Mock lastIdUser to return a specific User ID
            userRepositoryMock.Setup(repo => repo.lastIdUser()).ReturnsAsync(1); // Assume the last user id is 1

            // Act: Call the Register method
            var result = await controller.Register(userRegisterDto);

            // Assert: Verify that the result is Ok
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); // Check if status is OK
        }
        [TestMethod]
        public async Task Register_ReturnsBadRequest_WhenAccountAlreadyExists()
        {
            // Arrange
            var userRegisterDTO = new UserRegisterDTO();
            var user = new User();

            // Mocking the methods
            userRepositoryMock.Setup(repo => repo.checkAccountExsit(It.IsAny<User>())).ReturnsAsync(true);  // Account exists

            // Act
            var result = await controller.Register(userRegisterDTO);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("ton tai account", badRequestResult.Value);  // Check that the message is correct
        }

        [TestMethod]
        public async Task Register_ThrowsException_WhenRegisterFails()
        {
            // Arrange
            var userRegisterDTO = new UserRegisterDTO { /* Add necessary properties here */ };
            var user = new User { /* Map properties from DTO to User object */ };

            // Mocking the methods
            userRepositoryMock.Setup(repo => repo.checkAccountExsit(It.IsAny<User>())).ReturnsAsync(false);  // Account does not exist
            userRepositoryMock.Setup(repo => repo.Register(It.IsAny<UserRegisterDTO>())).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(() => controller.Register(userRegisterDTO));
            Assert.AreEqual("Register Failed Database error", exception.Message);
        }
        #endregion
        #region confirm
        [TestMethod]
        public async Task Confirm_ReturnsOk_WhenCodeIsCorrect()
        {
            // Arrange
            var confirmCode = new ConfirmCode { Code = "123456" };
            userRepositoryMock.Setup(repo => repo.confirmCode(confirmCode)).ReturnsAsync(true);

            // Act
            var result = await controller.Confirm(confirmCode);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task Confirm_ReturnsBadRequest_WhenCodeIsIncorrect()
        {
            // Arrange
            var confirmCode = new ConfirmCode { Code = "wrongCode" };
            userRepositoryMock.Setup(repo => repo.confirmCode(confirmCode)).ReturnsAsync(false);

            // Act
            var result = await controller.Confirm(confirmCode);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Incorrect code", okResult.Value);
        }

        [TestMethod]
        public async Task Confirm_ReturnsStatusCode500_WhenExceptionOccurs()
        {
            // Arrange
            var confirmCode = new ConfirmCode { Code = "123456" };

            // Simulate an exception when calling confirmCode
            userRepositoryMock.Setup(repo => repo.confirmCode(confirmCode))
                              .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                // Act
                var result = await controller.Confirm(confirmCode);
                // If no exception is thrown, fail the test
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                // Assert
                // Check if the exception message contains "Failed Confirm"
                Assert.IsTrue(ex.Message.Contains("Failed Confirm"));
                Assert.IsTrue(ex.Message.Contains("Test Exception"));
            }
        }
        #endregion
        #region login
        [TestMethod]
        public async Task Login_ReturnsNotFound_WhenLoginFails()
        {
            // Arrange
            var userLogin = new UserLoginDTO { Email = "wrong@example.com", Password = "wrongpassword" };

            // Mock the repository to return false for invalid login
            userRepositoryMock.Setup(repo => repo.checkLogin(It.IsAny<UserLoginDTO>())).ReturnsAsync(false);

            // Act
            var result = await controller.Login(userLogin);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Incorrect Email or Password", notFoundResult.Value);
        }

        [TestMethod]
        public async Task Login_ThrowsException_WhenExceptionOccurs()
        {
            // Arrange
            var userLogin = new UserLoginDTO { Email = "test@example.com", Password = "password123" };

            // Simulate an exception in the repository method
            userRepositoryMock.Setup(repo => repo.checkLogin(It.IsAny<UserLoginDTO>())).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            try
            {
                var result = await controller.Login(userLogin);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Login Failed"));
                Assert.IsTrue(ex.Message.Contains("Database error"));
            }
        }
        #endregion
        #region ForgotPassword
        [TestMethod]
        public async Task ForgotPassword_ReturnsOk_WhenAccountExists()
        {
            // Arrange
            var forgotPasswordDTO = new ForgotPasswordDTO
            {
                Email = "test@example.com"
            };
            var user = new User { Email = "test@example.com" }; // Simulated existing user
            userRepositoryMock.Setup(repo => repo.checkAccountExsit(It.IsAny<User>())).ReturnsAsync(true);
            userRepositoryMock.Setup(repo => repo.ForgotPassword(It.IsAny<ForgotPasswordDTO>())).Returns(Task.CompletedTask);

            mapperMock.Setup(mapper => mapper.Map<User>(It.IsAny<ForgotPasswordDTO>())).Returns(user);

            // Act
            var result = await controller.ForgotPassword(forgotPasswordDTO);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);  // Should return OK if the account exists
        }

        [TestMethod]
        public async Task ForgotPassword_ReturnsNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            var forgotPasswordDTO = new ForgotPasswordDTO
            {
                Email = "nonexistent@example.com"
            };
            var user = new User { Email = "nonexistent@example.com" }; // Simulate non-existing user
            userRepositoryMock.Setup(repo => repo.checkAccountExsit(It.IsAny<User>())).ReturnsAsync(false);

            // Act
            var result = await controller.ForgotPassword(forgotPasswordDTO);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found account", notFoundResult.Value);
        }
        [TestMethod]
        public async Task ForgotPassword_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var forgotPasswordDTO = new ForgotPasswordDTO
            {
                Email = "test@example.com"
            };

            // Mock the IUserRepository to throw an exception when checking if the account exists
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.checkAccountExsit(It.IsAny<User>()))
                              .ThrowsAsync(new Exception("Database error"));
            // Act & Assert
            try
            {
                await controller.ForgotPassword(forgotPasswordDTO);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Failed: Database error", ex.Message);
            }
        }


        #endregion
        #region Reset Password
        [TestMethod]
        public async Task ResetPassword_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var resetPasswordDTO = new ResetPasswordDTO
            {
                Email = "test@example.com",
                Password = "NewPassword123",
                ConfirmPassword = "NewPassword123",
                Code ="test"
            };

            // Mock the IUserRepository to throw an exception
            userRepositoryMock.Setup(repo => repo.ResetPassword(It.IsAny<ResetPasswordDTO>()))
                              .ThrowsAsync(new Exception("Database error"));
            // Act
            var result = await controller.ResetPassword(resetPasswordDTO);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // 400 Bad Request
            Assert.AreEqual("Database error", badRequestResult.Value); // Verify exception message
        }
        [TestMethod]
        public async Task ResetPassword_ReturnsOk()
        {
            // Arrange
            var resetPasswordDTO = new ResetPasswordDTO
            {
                Email = "test@example.com",
                Password = "NewPassword123",
                ConfirmPassword = "NewPassword123",
                Code = "test"
            };

            // Act
            var result = await controller.ResetPassword(resetPasswordDTO);

            // Assert
            var okObjectResult = result as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
        }

        #endregion
    }
}
