using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Test
{
    [TestClass]
    public class UserController_Test
    {
        private Mock<IUserRepository> _mockUserRepository;
        private UserController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Mock the repository
            _mockUserRepository = new Mock<IUserRepository>();

            _controller = new UserController(_mockUserRepository.Object, null); 
        }
        [TestMethod]
        public async Task EditProfile_ReturnsOk_WhenProfileIsUpdatedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var editProfileDTO = new EditProfileDTO
            {
                FullName = "Updated Name",
                Email = "updated.email@example.com"
            };

            var updatedUser = new Models.User { Id = userId, FullName = editProfileDTO.FullName, Email = editProfileDTO.Email };

            _mockUserRepository.Setup(repo => repo.EditProfile(It.IsAny<EditProfileDTO>()))
                .ReturnsAsync(updatedUser);  // Simulate successful profile update

            // Act
            var result = await _controller.EditProfile(userId, editProfileDTO);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task EditProfile_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = 1;
            var editProfileDTO = new EditProfileDTO
            {
                FullName = "Updated Name",
                Email = "updated.email@example.com"
            };

            _mockUserRepository.Setup(repo => repo.EditProfile(It.IsAny<EditProfileDTO>()))
                .ThrowsAsync(new Exception("Profile update failed"));  // Simulate an error

            // Act
            var result = await _controller.EditProfile(userId, editProfileDTO);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode);
            Assert.AreEqual("Error:  Profile update failed", statusCodeResult.Value);
        }
        [TestMethod]
        public async Task ChangePassword_ReturnsOk_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var changePasswordDTO = new ChangePasswordDTO
            {
                OldPassword = "oldPassword123",
                NewPassword = "newPassword456"
            };

            _mockUserRepository.Setup(repo => repo.ChangePassword(It.IsAny<ChangePasswordDTO>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(changePasswordDTO);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Change password successfully", okResult.Value);
        }

        [TestMethod]
        public async Task ChangePassword_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            var changePasswordDTO = new ChangePasswordDTO
            {
                OldPassword = "oldPassword123",
                NewPassword = "newPassword456"
            };

            _mockUserRepository.Setup(repo => repo.ChangePassword(It.IsAny<ChangePasswordDTO>()))
                .ThrowsAsync(new Exception("Something went wrong"));  // Simulate an error

            // Act
            var result = await _controller.ChangePassword(changePasswordDTO);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode);
            Assert.AreEqual("Error: Something went wrong", statusCodeResult.Value);
        }

    }
}
