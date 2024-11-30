using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.Infrastructure.Interfaces;
using System.Security.Principal;

namespace MyAPI.Test
{
    [TestClass]
    public class AccountController_Test
    {
        private Mock<IAccountRepository> serviceMock;
        private AccountController controller;
        private Mock<IMapper> mapperMock;

        [TestInitialize]
        public void Setup()
        {
            serviceMock = new Mock<IAccountRepository>();
            mapperMock = new Mock<IMapper>();
            controller = new AccountController(serviceMock.Object, mapperMock.Object);
        }
        #region get list account
        [TestMethod]
        public async Task GetListAccount_ReturnsOk_WithValidData()
        {
            // Arrange
            var mockAccounts = new List<AccountListDTO>
            {
            new AccountListDTO { FullName = "Nguyen Van A" },
            new AccountListDTO { FullName = "Le Van B" }
        };
            serviceMock.Setup(repo => repo.GetListAccount()).ReturnsAsync(mockAccounts);

            // Act
            var result = await controller.GetListAccount();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockAccounts, okResult.Value);
        }

        [TestMethod]
        public async Task GetListAccount_ReturnsNotFound_WhenNoData()
        {
            // Arrange
            serviceMock.Setup(repo => repo.GetListAccount()).ReturnsAsync((List<AccountListDTO>)null);

            // Act
            var result = await controller.GetListAccount();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found list Account", notFoundResult.Value);
        }
        [TestMethod]
        public async Task GetListAccount_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            var exceptionMessage = "Database connection error";
            serviceMock.Setup(repo => repo.GetListAccount()).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetListAccount();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);

            var response = badRequestResult.Value as dynamic;
            Assert.IsNotNull(response);
            Assert.AreEqual("Account get list failed", response.Message);
            Assert.AreEqual(exceptionMessage, response.Details);
        }
        #endregion
        #region get account detail

        [TestMethod]
        public async Task GetDetailsAccountById_ReturnsOk_WithValidId()
        {
            // Arrange
            var mockAccount = new AccountListDTO { FullName = "Nguyen Van A", NumberPhone = "123456789" };
            int accountId = 1;
            serviceMock.Setup(repo => repo.GetDetailsUser(accountId)).ReturnsAsync(mockAccount);

            // Act
            var result = await controller.GetDetailsAccountById(accountId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockAccount, okResult.Value);
        }

        // Test trường hợp tài khoản không tồn tại
        [TestMethod]
        public async Task GetDetailsAccountById_ReturnsNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            int accountId = 1;
            serviceMock.Setup(repo => repo.GetDetailsUser(accountId)).ReturnsAsync((AccountListDTO)null);

            // Act
            var result = await controller.GetDetailsAccountById(accountId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Account not exits", notFoundResult.Value);
        }

        // Test trường hợp ngoại lệ xảy ra
        [TestMethod]
        public async Task GetDetailsAccountById_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            int accountId = 1;
            var exceptionMessage = "Database connection error";
            serviceMock.Setup(repo => repo.GetDetailsUser(accountId)).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetDetailsAccountById(accountId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);

            var response = badRequestResult.Value as dynamic;
            Assert.IsNotNull(response);
            Assert.AreEqual("Account details failed", response.Message);
            Assert.AreEqual(exceptionMessage, response.Details);
        }
        #endregion
        #region delete account
        // Test trường hợp xóa thành công
        [TestMethod]
        public async Task DeleteAccountById_ReturnsOk_WhenAccountDeletedSuccessfully()
        {
            // Arrange
            int accountId = 1;
            serviceMock.Setup(repo => repo.DeteleAccount(accountId)).ReturnsAsync(true);

            // Act
            var result = await controller.DelteAccountById(accountId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Delte Success!!", okResult.Value);
        }

        // Test trường hợp không tìm thấy tài khoản
        [TestMethod]
        public async Task DeleteAccountById_ReturnsNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            int accountId = 1;
            serviceMock.Setup(repo => repo.DeteleAccount(accountId)).ReturnsAsync(false);

            // Act
            var result = await controller.DelteAccountById(accountId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Account not exits", notFoundResult.Value);
        }

        // Test trường hợp ngoại lệ xảy ra
        [TestMethod]
        public async Task DeleteAccountById_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            int accountId = 1;
            var exceptionMessage = "Database connection error";
            serviceMock.Setup(repo => repo.DeteleAccount(accountId)).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.DelteAccountById(accountId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);

            // Kiểm tra nội dung trả về
            var response = badRequestResult.Value as dynamic;
            Assert.IsNotNull(response);
            Assert.AreEqual("Account delete failed", response.Message);
            Assert.AreEqual(exceptionMessage, response.Details);
        }
        #endregion
        #region update account
        // Test trường hợp cập nhật tài khoản thành công
        [TestMethod]
        public async Task UpdateAccountById_ReturnsOk_WhenAccountUpdatedSuccessfully()
        {
            // Arrange
            int accountId = 1;
            int newIdUpdate = 2;
            serviceMock.Setup(repo => repo.UpdateRoleOfAccount(accountId, newIdUpdate)).ReturnsAsync(true);

            // Act
            var result = await controller.UpdateAccountById(accountId, newIdUpdate);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Account Update successfully.", (okResult.Value as dynamic).Message);
        }

        // Test trường hợp ngoại lệ xảy ra
        [TestMethod]
        public async Task UpdateAccountById_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            int accountId = 1;
            int newIdUpdate = 2;
            var exceptionMessage = "Database connection error";
            serviceMock.Setup(repo => repo.UpdateRoleOfAccount(accountId, newIdUpdate)).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.UpdateAccountById(accountId, newIdUpdate);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);

            // Kiểm tra nội dung trả về
            var response = badRequestResult.Value as dynamic;
            Assert.IsNotNull(response);
            Assert.AreEqual("Account update failed", response.Message);
            Assert.AreEqual(exceptionMessage, response.Details);
        }
        #endregion
        #region list role
        [TestMethod]
        public async Task GetListRole_ReturnsOk_WhenRolesExist()
        {
            // Arrange
            var mockRoles = new List<AccountRoleDTO>
            {
            new AccountRoleDTO {Id = 1, RoleName ="Manage"},
            new AccountRoleDTO { Id = 1, RoleName ="User" }
        };
            serviceMock.Setup(repo => repo.GetListRole()).ReturnsAsync(mockRoles);

            // Act
            var result = await controller.GetListRole();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockRoles, okResult.Value);
        }

        [TestMethod]
        public async Task GetListRole_ReturnsNotFound_WhenNoRolesExist()
        {
            // Arrange
            serviceMock.Setup(repo => repo.GetListRole()).ReturnsAsync((List<AccountRoleDTO>)null);

            // Act
            var result = await controller.GetListRole();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found list Role", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetListRole_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            serviceMock.Setup(repo => repo.GetListRole()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetListRole();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Account list failed", (badRequestResult.Value as dynamic).Message);
            Assert.AreEqual("Database error", (badRequestResult.Value as dynamic).Details);
        }
        #endregion
    }
}
