using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class TripController_Test
    {
        private Mock<ITripRepository> _mockTripRepository;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<GetInforFromToken> _mockGetInforFromToken;
        private TripController _tripController;

        [TestInitialize]
        public void Setup()
        {
            _mockTripRepository = new Mock<ITripRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockGetInforFromToken = new Mock<GetInforFromToken>();
            _tripController = new TripController(_mockTripRepository.Object, _mockHttpContextAccessor.Object, _mockGetInforFromToken.Object);
        }
        [TestMethod]
        public async Task GetListTrip_ReturnsOk_WithValidTrips()
        {

            // Set up the mock to return a list of trips
            var trips = new List<TripDTO>
                {
                    new TripDTO { Id = 1, Name = "Trip to Hanoi", StartTime = new TimeSpan(8, 0, 0), Description = "Description", Price = 100.00m, PointStart = "HCM", PointEnd = "Hanoi", Status = true },
                    new TripDTO { Id = 2, Name = "Trip to Da Nang", StartTime = new TimeSpan(9, 0, 0), Description = "Description", Price = 80.00m, PointStart = "HCM", PointEnd = "Da Nang", Status = true }
                };

            _mockTripRepository.Setup(repo => repo.GetListTrip()).ReturnsAsync(trips);

            // Act
            var result = await _tripController.GetListTrip();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var resultTrips = okResult.Value as List<TripDTO>;
            Assert.IsNotNull(resultTrips);
            Assert.AreEqual(2, resultTrips.Count);
        }
        [TestMethod]
        public async Task GetListTrip_ReturnsNotFound_WhenNoTrips()
        {

            // Set up the mock to return an empty list
            _mockTripRepository.Setup(repo => repo.GetListTrip()).ReturnsAsync((List<TripDTO>) null);

            // Act
            var result = await _tripController.GetListTrip();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found any trip", notFoundResult.Value);
        }
        [TestMethod]
        public async Task GetListTrip_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var mockTripRepository = new Mock<ITripRepository>();
            var controller = new TripController(mockTripRepository.Object, null, null);

            mockTripRepository.Setup(repo => repo.GetListTrip())
                              .ThrowsAsync(new Exception("Database error"));

            try
            {
                // Act
                var result = await controller.GetListTrip();
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.Contains("GetListTrip"));
                Assert.IsTrue(ex.Message.Contains("Database error"));
            }
        }
        [TestMethod]
        public async Task SearchTrip_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var startPoint = "New York";
            var endPoint = "Los Angeles";
            var time = DateTime.Now;
            var mockSession = new Mock<ISession>();
            _mockHttpContextAccessor.Setup(h => h.HttpContext.Session).Returns(mockSession.Object);

            _mockTripRepository.Setup(repo => repo.SreachTrip(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                              .ThrowsAsync(new Exception("Test Exception"));
            var result = await _tripController.searchTrip(startPoint, endPoint, time);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult); // Ensure result is a BadRequestObjectResult
            Assert.AreEqual(400, badRequestResult.StatusCode); // Ensure status code is 400
            Assert.AreEqual("searchTripAPI: Test Exception", badRequestResult.Value);
        }
        [TestMethod]
        public async Task SearchTrip_ReturnsOk_WhenTripsAreFound()
        {
            // Arrange
            var startPoint = "New York";
            var endPoint = "Los Angeles";
            var time = DateTime.Now;

            // Create a mock list of trips to simulate the result
            var trips = new List<TripVehicleDTO>
            {
                new TripVehicleDTO { Id = 1 , StartTime = TimeSpan.FromHours(9), PointStart = startPoint, PointEnd = endPoint },
                new TripVehicleDTO { Id = 2, StartTime = TimeSpan.FromHours(12), PointStart = startPoint, PointEnd = endPoint }
            };
            var mockSession = new Mock<ISession>();
            _mockHttpContextAccessor.Setup(h => h.HttpContext.Session).Returns(mockSession.Object);

            _mockTripRepository.Setup(repo => repo.SreachTrip(startPoint, endPoint, It.IsAny<string>()))
                              .ReturnsAsync(trips);

            // Act
            var result = await _tripController.searchTrip(startPoint, endPoint, time);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult); // Ensure the result is OkObjectResult
            Assert.AreEqual(200, okResult.StatusCode); // Ensure status code is 200 (OK)
            Assert.AreEqual(trips, okResult.Value); // Ensure the result contains the list of trips
        }
        [TestMethod]
        public async Task SearchTrip_ReturnsNotFound_WhenNoTripsAreFound()
        {
            // Arrange
            var startPoint = "New York";
            var endPoint = "Los Angeles";
            var time = DateTime.Now;
            var mockSession = new Mock<ISession>();
            _mockHttpContextAccessor.Setup(h => h.HttpContext.Session).Returns(mockSession.Object);

            _mockTripRepository.Setup(repo => repo.SreachTrip(startPoint, endPoint, It.IsAny<string>()))
                              .ReturnsAsync((List<TripVehicleDTO>)null);

            // Act
            var result = await _tripController.searchTrip(startPoint, endPoint, time);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found trip", notFoundResult.Value);
        }
        [TestMethod]
        public async Task AddTrip_ReturnsBadRequest_WhenTokenIsMissing()
        {
            // Arrange
            var trip = new TripDTO { };
            int vehicleId = 1;
            // Act
            var result = await _tripController.addTrip(trip, vehicleId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult); // Ensure the result is BadRequest
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

    }
}
