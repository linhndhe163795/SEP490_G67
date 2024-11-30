using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class TypeOfDriverController_Test
    {
        private Mock<ITypeOfDriverRepository> _mockRepository;
        private TypeOfDriverController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ITypeOfDriverRepository>();
            _controller = new TypeOfDriverController(_mockRepository.Object);
        }
        [TestMethod]
        public async Task CreateTypeOfDriver_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Arrange
            UpdateTypeOfDriverDTO updateTypeOfDriverDto = null;

            // Act
            var result = await _controller.CreateTypeOfDriver(updateTypeOfDriverDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid data", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateTypeOfDriver_ReturnsCreatedAtAction_WhenCreationIsSuccessful()
        {
            // Arrange
            var updateTypeOfDriverDto = new UpdateTypeOfDriverDTO { Description = "New Driver Type" };
            var createdTypeOfDriver = new TypeOfDriver { Id = 1};

            _mockRepository.Setup(repo => repo.CreateTypeOfDriverAsync(updateTypeOfDriverDto))
                           .ReturnsAsync(createdTypeOfDriver);

            // Act
            var result = await _controller.CreateTypeOfDriver(updateTypeOfDriverDto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual("GetTypeOfDriverById", createdResult.ActionName);
            Assert.AreEqual(createdTypeOfDriver.Id, createdResult.RouteValues["id"]);
            Assert.AreEqual(createdTypeOfDriver, createdResult.Value);
        }
        [TestMethod]
        public async Task UpdateTypeOfDriver_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Arrange
            UpdateTypeOfDriverDTO updateTypeOfDriverDto = null;
            int id = 1;

            // Act
            var result = await _controller.UpdateTypeOfDriver(id, updateTypeOfDriverDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid data", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateTypeOfDriver_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var id = 1;
            var updateTypeOfDriverDto = new UpdateTypeOfDriverDTO { Description = "Updated Driver Type" };
            var updatedTypeOfDriver = new TypeOfDriver { Id = id, Description = "Updated Driver Type" };

            _mockRepository.Setup(repo => repo.UpdateTypeOfDriverAsync(id, updateTypeOfDriverDto))
                           .ReturnsAsync(updatedTypeOfDriver);

            // Act
            var result = await _controller.UpdateTypeOfDriver(id, updateTypeOfDriverDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(updatedTypeOfDriver, okResult.Value);
        }

        [TestMethod]
        public async Task UpdateTypeOfDriver_ReturnsNotFound_WhenKeyNotFoundExceptionOccurs()
        {
            // Arrange
            var id = 1;
            var updateTypeOfDriverDto = new UpdateTypeOfDriverDTO { Description = "Updated Driver Type" };
            _mockRepository.Setup(repo => repo.UpdateTypeOfDriverAsync(id, updateTypeOfDriverDto))
                           .ThrowsAsync(new KeyNotFoundException("TypeOfDriver not found"));

            // Act
            var result = await _controller.UpdateTypeOfDriver(id, updateTypeOfDriverDto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("TypeOfDriver not found", notFoundResult.Value);
        }
        [TestMethod]
        public async Task DeleteTypeOfDriver_ReturnsNotFound_WhenTypeOfDriverDoesNotExist()
        {
            // Arrange
            int id = 1;
            _mockRepository.Setup(repo => repo.Get(id))
                           .ReturnsAsync((TypeOfDriver)null);

            // Act
            var result = await _controller.DeleteTypeOfDriver(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Type of driver not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task DeleteTypeOfDriver_ReturnsOk_WhenTypeOfDriverIsDeletedSuccessfully()
        {
            // Arrange
            int id = 1;
            var typeOfDriver = new TypeOfDriver { Id = id, Description = "Driver Type 1" };
            _mockRepository.Setup(repo => repo.Get(id))
                           .ReturnsAsync(typeOfDriver); 

            // Act
            var result = await _controller.DeleteTypeOfDriver(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Deleted successfully", okResult.Value);
        }
        [TestMethod]
        public async Task GetAllTypeOfDrivers_ReturnsOk_WhenTypesOfDriversExist()
        {
            // Arrange
            var typesOfDrivers = new List<TypeOfDriver>
        {
            new TypeOfDriver { Id = 1, Description = "Driver Type 1" },
            new TypeOfDriver { Id = 2, Description = "Driver Type 2" }
        };

            _mockRepository.Setup(repo => repo.GetAll())
                           .ReturnsAsync(typesOfDrivers);

            // Act
            var result = await _controller.GetAllTypeOfDrivers();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(typesOfDrivers, okResult.Value);
        }

        [TestMethod]
        public async Task GetAllTypeOfDrivers_ReturnsOk_WhenNoTypesOfDriversExist()
        {
            // Arrange
            var typesOfDrivers = new List<TypeOfDriver>();

            _mockRepository.Setup(repo => repo.GetAll())
                           .ReturnsAsync(typesOfDrivers);

            // Act
            var result = await _controller.GetAllTypeOfDrivers();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(typesOfDrivers, okResult.Value); 
        }

    }
}
