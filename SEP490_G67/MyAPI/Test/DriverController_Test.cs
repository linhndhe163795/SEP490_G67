using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class DriverController_Test
    {
        private Mock<IDriverRepository> driverRepositoryMock;
        private Mock<ITypeOfDriverRepository> typeOfDriverRepositoryMock;
        private Mock<IMapper> mapperMock;
        private DriverController controller;

        [TestInitialize]
        public void SetUp()
        {
            driverRepositoryMock = new Mock<IDriverRepository>();
            typeOfDriverRepositoryMock = new Mock<ITypeOfDriverRepository>();
            mapperMock = new Mock<IMapper>();

            controller = new DriverController(driverRepositoryMock.Object, typeOfDriverRepositoryMock.Object, mapperMock.Object);
        }
        #region GetAllDrivers

        [TestMethod]
        public async Task GetAllDrivers_ReturnsOkResult_WithDriverDTOs()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                new Driver { Id = 1, Name = "Nguyen Van A" },
                new Driver { Id = 2, Name = "Nguyen Van B" }
            };

            var driverDTOs = new List<DriverDTO>
            {
                new DriverDTO { Id = "1", Name = "Nguyen Van A" },
                new DriverDTO { Id = "2", Name = "Nguyen Van B" }
            };

            driverRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(drivers);
            mapperMock.Setup(mapper => mapper.Map<IEnumerable<DriverDTO>>(drivers)).Returns(driverDTOs);

            // Act
            var result = await controller.GetAllDrivers();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedData = okResult.Value as IEnumerable<DriverDTO>;
            Assert.IsNotNull(returnedData);
            CollectionAssert.AreEqual(driverDTOs, (List<DriverDTO>)returnedData);

            driverRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<IEnumerable<DriverDTO>>(drivers), Times.Once);
        }
        #endregion
        #region GetDriverById
        [TestMethod]
        public async Task GetDriverById_ReturnsOkResult_WithDriverDTO()
        {
            // Arrange
            int driverId = 1;
            var driver = new Driver { Id = driverId, Name = "Nguyen Van A" };
            var driverDTO = new DriverDTO { Id = "1", Name = "Nguyen Van A" };

            driverRepositoryMock.Setup(repo => repo.Get(driverId)).ReturnsAsync(driver);
            mapperMock.Setup(mapper => mapper.Map<DriverDTO>(driver)).Returns(driverDTO);

            // Act
            var result = await controller.GetDriverById(driverId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedData = okResult.Value as DriverDTO;
            Assert.IsNotNull(returnedData);
            Assert.AreEqual(driverDTO.Id, returnedData.Id);
            Assert.AreEqual(driverDTO.Name, returnedData.Name);

            driverRepositoryMock.Verify(repo => repo.Get(driverId), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<DriverDTO>(driver), Times.Once);
        }

        [TestMethod]
        public async Task GetDriverById_ReturnsNotFound_WhenDriverDoesNotExist()
        {
            // Arrange
            int driverId = 1;

            driverRepositoryMock.Setup(repo => repo.Get(driverId)).ReturnsAsync((Driver)null);

            // Act
            var result = await controller.GetDriverById(driverId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Driver not found", notFoundResult.Value);

            driverRepositoryMock.Verify(repo => repo.Get(driverId), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<DriverDTO>(It.IsAny<Driver>()), Times.Never);
        }

        #endregion
        #region CreateDriver
        [TestMethod]
        public async Task CreateDriver_ReturnsCreatedAtAction_WithDriverDTO()
        {
            // Arrange
            var updateDriverDto = new UpdateDriverDTO { Name = "Nguyen Van A" };
            var driver = new Driver { Id = 1, Name = "Nguyen Van A" };
            var createdDriverDto = new UpdateDriverDTO { Name = "Nguyen Van A" };

            driverRepositoryMock.Setup(repo => repo.CreateDriverAsync(updateDriverDto)).ReturnsAsync(driver);
            mapperMock.Setup(mapper => mapper.Map<UpdateDriverDTO>(driver)).Returns(createdDriverDto);

            // Act
            var result = await controller.CreateDriver(updateDriverDto);

            // Assert
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(201, createdAtActionResult.StatusCode);
            Assert.AreEqual("GetDriverById", createdAtActionResult.ActionName);

            var returnedDto = createdAtActionResult.Value as UpdateDriverDTO;
            Assert.IsNotNull(returnedDto);
            Assert.AreEqual(createdDriverDto.Name, returnedDto.Name);

            driverRepositoryMock.Verify(repo => repo.CreateDriverAsync(updateDriverDto), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<UpdateDriverDTO>(driver), Times.Once);
        }

        [TestMethod]
        public async Task CreateDriver_ReturnsBadRequest_WhenDriverDtoIsNull()
        {
            // Act
            var result = await controller.CreateDriver(null);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid driver data", badRequestResult.Value);

            driverRepositoryMock.Verify(repo => repo.CreateDriverAsync(It.IsAny<UpdateDriverDTO>()), Times.Never);
            mapperMock.Verify(mapper => mapper.Map<UpdateDriverDTO>(It.IsAny<Driver>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateDriver_ReturnsBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var updateDriverDto = new UpdateDriverDTO { Name = "Invalid Driver" };
            driverRepositoryMock
                .Setup(repo => repo.CreateDriverAsync(updateDriverDto))
                .ThrowsAsync(new ArgumentException("Invalid driver data"));

            // Act
            var result = await controller.CreateDriver(updateDriverDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid driver data", badRequestResult.Value);

            driverRepositoryMock.Verify(repo => repo.CreateDriverAsync(updateDriverDto), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<UpdateDriverDTO>(It.IsAny<Driver>()), Times.Never);
        }
        #endregion
        #region UpdateDriver
        [TestMethod]
        public async Task UpdateDriver_ReturnsOk_WithUpdatedDriver()
        {
            // Arrange
            int driverId = 1;
            var updateDriverDto = new UpdateDriverDTO { Name = "Updated Driver" };
            var updatedDriver = new Driver { Id = driverId, Name = "Updated Driver" };

            driverRepositoryMock
                .Setup(repo => repo.UpdateDriverAsync(driverId, updateDriverDto))
                .ReturnsAsync(updatedDriver);

            // Act
            var result = await controller.UpdateDriver(driverId, updateDriverDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedDriver = okResult.Value as Driver;
            Assert.IsNotNull(returnedDriver);
            Assert.AreEqual(updatedDriver.Name, returnedDriver.Name);

            driverRepositoryMock.Verify(repo => repo.UpdateDriverAsync(driverId, updateDriverDto), Times.Once);
        }

        [TestMethod]
        public async Task UpdateDriver_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await controller.UpdateDriver(1, null);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid driver data", badRequestResult.Value);

            driverRepositoryMock.Verify(repo => repo.UpdateDriverAsync(It.IsAny<int>(), It.IsAny<UpdateDriverDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateDriver_ReturnsNotFound_WhenKeyNotFoundExceptionIsThrown()
        {
            // Arrange
            int driverId = 999;
            var updateDriverDto = new UpdateDriverDTO { Name = "Non-Existent Driver" };

            driverRepositoryMock
                .Setup(repo => repo.UpdateDriverAsync(driverId, updateDriverDto))
                .ThrowsAsync(new KeyNotFoundException("Driver not found"));

            // Act
            var result = await controller.UpdateDriver(driverId, updateDriverDto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Driver not found", notFoundResult.Value);

            driverRepositoryMock.Verify(repo => repo.UpdateDriverAsync(driverId, updateDriverDto), Times.Once);
        }
        #endregion
        #region DeleteDriver
        [TestMethod]
        public async Task DeleteDriver_ReturnsOk_WhenDriverIsDeleted()
        {
            // Arrange
            int driverId = 1;
            var driver = new Driver { Id = driverId, Name = "Test Driver" };

            driverRepositoryMock.Setup(repo => repo.Get(driverId)).ReturnsAsync(driver);

            // Act
            var result = await controller.DeleteDriver(driverId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Driver deleted successfully", okResult.Value);

            driverRepositoryMock.Verify(repo => repo.Get(driverId), Times.Once);
            driverRepositoryMock.Verify(repo => repo.Delete(driver), Times.Once);
        }

        [TestMethod]
        public async Task DeleteDriver_ReturnsNotFound_WhenDriverDoesNotExist()
        {
            // Arrange
            int driverId = 999;

            driverRepositoryMock.Setup(repo => repo.Get(driverId)).ReturnsAsync((Driver)null);

            // Act
            var result = await controller.DeleteDriver(driverId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Driver not found", notFoundResult.Value);

            driverRepositoryMock.Verify(repo => repo.Get(driverId), Times.Once);
            driverRepositoryMock.Verify(repo => repo.Delete(It.IsAny<Driver>()), Times.Never);
        }
        #endregion
    }
}
