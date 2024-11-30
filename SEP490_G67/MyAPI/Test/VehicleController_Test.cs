using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Test
{
    [TestClass]
    public class VehicleController_Test
    {
        private Mock<IVehicleRepository> _mockVehicleRepository;
        private Mock<IMapper> _mockMapper;
        private VehicleController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockVehicleRepository = new Mock<IVehicleRepository>();
            _mockMapper = new Mock<IMapper>();
            _controller = new VehicleController(_mockVehicleRepository.Object, _mockMapper.Object);
        }
        [TestMethod]
        public async Task GetVehicleType_ReturnsOk_WhenVehicleTypesExist()
        {
            // Arrange
            var vehicleTypes = new List<VehicleTypeDTO>
            {
                new VehicleTypeDTO { Id = 1, Description = "Sedan" },
                new VehicleTypeDTO { Id = 2, Description = "SUV" }
            };

            _mockVehicleRepository.Setup(repo => repo.GetVehicleTypeDTOsAsync())
                                  .ReturnsAsync(vehicleTypes);

            // Act
            var result = await _controller.GetVehicleType();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(vehicleTypes, okResult.Value);
        }
        [TestMethod]
        public async Task GetVehicleType_ReturnsNotFound_WhenNoVehicleTypesExist()
        {
            // Arrange
            _mockVehicleRepository.Setup(repo => repo.GetVehicleTypeDTOsAsync())
                                  .ReturnsAsync((List<VehicleTypeDTO>)null); // No vehicle types

            // Act
            var result = await _controller.GetVehicleType();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Vehicle Type not found", notFoundResult.Value);
        }
        [TestMethod]
        public async Task GetVehicleType_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockVehicleRepository.Setup(repo => repo.GetVehicleTypeDTOsAsync())
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetVehicleType();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }
        [TestMethod]
        public async Task GetVehicleList_ReturnsOk_WhenVehiclesExist()
        {
            // Arrange
            var vehicleList = new List<VehicleListDTO>
            {
                new VehicleListDTO { Id = 1, Description = "Vehicle 1" },
                new VehicleListDTO { Id = 2, Description = "Vehicle 2" }
            };

            _mockVehicleRepository.Setup(repo => repo.GetVehicleDTOsAsync())
                                  .ReturnsAsync(vehicleList);

            // Act
            var result = await _controller.GetVehicleList();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(vehicleList, okResult.Value);
        }
        [TestMethod]
        public async Task GetVehicleList_ReturnsNotFound_WhenNoVehiclesExist()
        {
            // Arrange
            _mockVehicleRepository.Setup(repo => repo.GetVehicleDTOsAsync())
                                  .ReturnsAsync((List<VehicleListDTO>)null); // No vehicles

            // Act
            var result = await _controller.GetVehicleList();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Vehicle list not found", notFoundResult.Value);
        }
        [TestMethod]
        public async Task GetVehicleList_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockVehicleRepository.Setup(repo => repo.GetVehicleDTOsAsync())
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetVehicleList();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }
        [TestMethod]
        public async Task AddVehicle_ReturnsOk_WhenVehicleAddedSuccessfully()
        {
            // Arrange
            var vehicleAddDTO = new VehicleAddDTO { Description = "Vehicle 1"};
            var driverName = "NVA";
            var roleId = 1;

            _mockVehicleRepository.Setup(repo => repo.AddVehicleAsync(vehicleAddDTO, driverName))
                                  .ReturnsAsync(true);

            // Act
            var result = await _controller.AddVehicle(vehicleAddDTO, driverName, roleId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Vehicle added successfully.", (okResult.Value as dynamic).Message);
            Assert.AreEqual(vehicleAddDTO, (okResult.Value as dynamic).Vehicle);
        }
        [TestMethod]
        public async Task AddVehicle_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var vehicleAddDTO = new VehicleAddDTO { Description = "Vehicle 1" };
            var driverName = "NVA";
            var roleId = 1;

            _mockVehicleRepository.Setup(repo => repo.AddVehicleAsync(vehicleAddDTO, driverName))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.AddVehicle(vehicleAddDTO, driverName, roleId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("AddVehicle failed", (badRequestResult.Value as dynamic).Message);
            Assert.AreEqual("Database error", (badRequestResult.Value as dynamic).Details);
        }
        [TestMethod]
        public async Task AddVehicleByStaff_ReturnsOk_WhenVehicleAddedSuccessfully()
        {
            // Arrange
            var requestID = 1;
            var isApprove = true;

            _mockVehicleRepository.Setup(repo => repo.AddVehicleByStaffcheckAsync(requestID, isApprove))
                                  .ReturnsAsync(true);

            // Act
            var result = await _controller.AddVehicleByStaff(requestID, isApprove);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Vehicle added successfully.", (okResult.Value as dynamic).Message);
        }
        [TestMethod]
        public async Task AddVehicleByStaff_ReturnsBadRequest_WhenVehicleAdditionDenied()
        {
            // Arrange
            var requestID = 1;
            var isApprove = false;

            _mockVehicleRepository.Setup(repo => repo.AddVehicleByStaffcheckAsync(requestID, isApprove))
                                  .ReturnsAsync(false);

            // Act
            var result = await _controller.AddVehicleByStaff(requestID, isApprove);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Vehicle addition denied.", (badRequestResult.Value as dynamic).Message);
        }
        [TestMethod]
        public async Task AddVehicleByStaff_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var requestID = 1;
            var isApprove = true;

            _mockVehicleRepository.Setup(repo => repo.AddVehicleByStaffcheckAsync(requestID, isApprove))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.AddVehicleByStaff(requestID, isApprove);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("AddVehicle By Staff failed", (badRequestResult.Value as dynamic).Message);
            Assert.AreEqual("Database error", (badRequestResult.Value as dynamic).Details);
        }
        [TestMethod]
        public async Task UpdateVehicle_ReturnsOk_WhenVehicleUpdatedSuccessfully()
        {
            // Arrange
            var id = 1;
            var driverName = "NVA";

            _mockVehicleRepository.Setup(repo => repo.UpdateVehicleAsync(id, driverName))
                                  .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateVehicle(id, driverName);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Vehicle Update successfully.", (okResult.Value as dynamic).Message);
        }
        [TestMethod]
        public async Task UpdateVehicle_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = 1;
            var driverName = "NVA";

            _mockVehicleRepository.Setup(repo => repo.UpdateVehicleAsync(id, driverName))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateVehicle(id, driverName);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }
        [TestMethod]
        public async Task UpdateVehicle_ReturnsOk_WhenVehicleDeletedSuccessfully()
        {
            // Arrange
            var id = 1;

            _mockVehicleRepository.Setup(repo => repo.DeleteVehicleAsync(id))
                                  .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateVehicle(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Vehicle delete successfully.", (okResult.Value as dynamic).Message);
        }
        [TestMethod]
        public async Task UpdateVehicle_ReturnsBadRequest_WhenVehicleDeletionFails()
        {
            // Arrange
            var id = 1;

            _mockVehicleRepository.Setup(repo => repo.DeleteVehicleAsync(id))
                                  .ReturnsAsync(false); 

            // Act
            var result = await _controller.UpdateVehicle(id);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Vehicle delete failed.", (badRequestResult.Value as dynamic).Message);
        }
        [TestMethod]
        public async Task GetStartPointTripFromVehicle_ReturnsOk_WhenListIsFound()
        {
            // Arrange
            var vehicleId = 1;
            var startPointList = new List<StartPointDTO> { new StartPointDTO() { id=1} }; 

            _mockVehicleRepository.Setup(repo => repo.GetListStartPointByVehicleId(vehicleId))
                                  .ReturnsAsync(startPointList); 

            // Act
            var result = await _controller.getStartPointTripFromVehicle(vehicleId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(startPointList, okResult.Value);
        }
        [TestMethod]
        public async Task GetStartPointTripFromVehicle_ReturnsNotFound_WhenNoStartPointsFound()
        {
            // Arrange
            var vehicleId = 1;

            _mockVehicleRepository.Setup(repo => repo.GetListStartPointByVehicleId(vehicleId))
                                  .ReturnsAsync((List<StartPointDTO>)null);

            // Act
            var result = await _controller.getStartPointTripFromVehicle(vehicleId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [TestMethod]
        public async Task GetStartPointTripFromVehicle_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var vehicleId = 1;

            _mockVehicleRepository.Setup(repo => repo.GetListStartPointByVehicleId(vehicleId))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.getStartPointTripFromVehicle(vehicleId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Database error", (badRequestResult.Value as dynamic).Message);
        }
        [TestMethod]
        public async Task GetEndPointTripFromVehicle_ReturnsOk_WhenListIsFound()
        {
            // Arrange
            var vehicleId = 1;
            var endPointList = new List<EndPointDTO> { new EndPointDTO() { id = 1 } }; 

            _mockVehicleRepository.Setup(repo => repo.GetListEndPointByVehicleId(vehicleId))
                                  .ReturnsAsync(endPointList);

            // Act
            var result = await _controller.getEndPointTripFromVehicle(vehicleId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(endPointList, okResult.Value);
        }
        [TestMethod]
        public async Task GetEndPointTripFromVehicle_ReturnsNotFound_WhenNoEndPointsFound()
        {
            // Arrange
            var vehicleId = 1;

            _mockVehicleRepository.Setup(repo => repo.GetListEndPointByVehicleId(vehicleId))
                                  .ReturnsAsync((List<EndPointDTO>)null);

            // Act
            var result = await _controller.getEndPointTripFromVehicle(vehicleId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [TestMethod]
        public async Task GetEndPointTripFromVehicle_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var vehicleId = 1;

            _mockVehicleRepository.Setup(repo => repo.GetListEndPointByVehicleId(vehicleId))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.getEndPointTripFromVehicle(vehicleId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Database error", (badRequestResult.Value as dynamic).Message);
        }


    }
}
