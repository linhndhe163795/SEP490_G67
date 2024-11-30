using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class TripDetailsController_Test
    {
        private Mock<ITripDetailsRepository> _mockTripDetailsRepository;
        private TripDetailsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockTripDetailsRepository = new Mock<ITripDetailsRepository>();
            _controller = new TripDetailsController(_mockTripDetailsRepository.Object);
        }
        [TestMethod]
        public async Task GetListTripDetailsbyTripId_ReturnsOk_WhenTripDetailsAreFound()
        {
            // Arrange
            var tripId = 1;
            var tripDetails = new List<TripDetailsDTO> 
        {
            new TripDetailsDTO { TripId = tripId}
        };
            _mockTripDetailsRepository.Setup(repo => repo.TripDetailsByTripId(tripId))
                                      .ReturnsAsync(tripDetails);

            // Act
            var result = await _controller.getListTripDetailsbyTripId(tripId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(tripDetails, okResult.Value);
        }

        [TestMethod]
        public async Task GetListTripDetailsbyTripId_ReturnsNotFound_WhenNoTripDetailsFound()
        {
            // Arrange
            var tripId = 1;
            _mockTripDetailsRepository.Setup(repo => repo.TripDetailsByTripId(tripId))
                                      .ReturnsAsync((List<TripDetailsDTO>)null);

            // Act
            var result = await _controller.getListTripDetailsbyTripId(tripId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetListTripDetailsbyTripId_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var tripId = 1;

            _mockTripDetailsRepository.Setup(repo => repo.TripDetailsByTripId(tripId))
                                      .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.getListTripDetailsbyTripId(tripId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("getListTripDetailsbyTripId API: Database error", badRequestResult.Value);
        }
        [TestMethod]
        public async Task GetListstartPointTripDetailsbyTripId_ReturnsOk_WhenTripDetailsAreFound()
        {
            // Arrange
            var tripId = 1;
            var tripDetails = new List<StartPointTripDetails> // Assuming TripDetail is the model type
        {
            new StartPointTripDetails { PointStartDetails  = "1"}
        };

            _mockTripDetailsRepository.Setup(repo => repo.StartPointTripDetailsById(tripId))
                                      .ReturnsAsync(tripDetails);

            // Act
            var result = await _controller.getListstartPointTripDetailsbyTripId(tripId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(tripDetails, okResult.Value);
        }

        [TestMethod]
        public async Task GetListstartPointTripDetailsbyTripId_ReturnsNotFound_WhenNoTripDetailsFound()
        {
            // Arrange
            var tripId = 1;

            _mockTripDetailsRepository.Setup(repo => repo.StartPointTripDetailsById(tripId))
                                      .ReturnsAsync((List<StartPointTripDetails>)null);

            // Act
            var result = await _controller.getListstartPointTripDetailsbyTripId(tripId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetListstartPointTripDetailsbyTripId_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var tripId = 1;

            _mockTripDetailsRepository.Setup(repo => repo.StartPointTripDetailsById(tripId))
                                      .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.getListstartPointTripDetailsbyTripId(tripId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("getListTripDetailsbyTripId API: Database error", badRequestResult.Value);
        }
        [TestMethod]
        public async Task GetListendPointTripDetailsbyTripId_ReturnsOk_WhenTripDetailsAreFound()
        {
            // Arrange
            var tripId = 1;
            var tripDetails = new List<EndPointTripDetails> // Assuming TripDetail is the model type
            {
                new EndPointTripDetails { PointEndDetails ="1"}
            };

            _mockTripDetailsRepository.Setup(repo => repo.EndPointTripDetailsById(tripId))
                                      .ReturnsAsync(tripDetails);

            // Act
            var result = await _controller.getListendPointTripDetailsbyTripId(tripId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(tripDetails, okResult.Value);
        }

        [TestMethod]
        public async Task GetListendPointTripDetailsbyTripId_ReturnsNotFound_WhenNoTripDetailsFound()
        {
            // Arrange
            var tripId = 1;

            _mockTripDetailsRepository.Setup(repo => repo.EndPointTripDetailsById(tripId))
                                      .ReturnsAsync((List<EndPointTripDetails>)null);

            // Act
            var result = await _controller.getListendPointTripDetailsbyTripId(tripId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetListendPointTripDetailsbyTripId_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var tripId = 1;

            _mockTripDetailsRepository.Setup(repo => repo.EndPointTripDetailsById(tripId))
                                      .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.getListendPointTripDetailsbyTripId(tripId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("getListTripDetailsbyTripId API: Database error", badRequestResult.Value);
        }

    }
}
