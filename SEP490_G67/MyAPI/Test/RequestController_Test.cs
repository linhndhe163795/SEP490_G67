using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class RequestController_Test
    {
        private Mock<IRequestRepository> _mockRequestRepository;
        private RequestController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRequestRepository = new Mock<IRequestRepository>();
            _controller = new RequestController(_mockRequestRepository.Object);
        }
        #region GetAllRequests
        [TestMethod]
        public async Task GetAllRequests_ReturnsOkResult_WithListOfRequests()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var mockRequests = new List<Request>
        {
            new Request { Id = 1, UserId = 101, Status = true, Description = "Request 1" },
            new Request { Id = 2, UserId = 102, Status = false, Description = "Request 2" }
        };

            // Set up the repository to return the mock list
            mockRequestRepository.Setup(repo => repo.GetAll()).ReturnsAsync(mockRequests);

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.GetAllRequests();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnValue = okResult.Value as List<Request>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count); // Ensure there are two requests
        }

        // Test: When there are no requests (empty list)
        [TestMethod]
        public async Task GetAllRequests_ReturnsOk_EmptyList()
        {
            // Arrange
            var mockRequests = new List<Request>(); // Empty list of requests

            // Set up the mock repository to return an empty list
            _mockRequestRepository.Setup(repo => repo.GetAll()).ReturnsAsync(mockRequests);

            // Act
            var result = await _controller.GetAllRequests();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); // Status should still be 200 OK
            var returnValue = okResult.Value as List<Request>;
            Assert.AreEqual(0, returnValue.Count); // The returned list should be empty
        }

        #endregion
        #region GetRequestById
        [TestMethod]
        public async Task GetRequestById_ReturnsOk_WhenRequestExists()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var requestId = 1;
            var mockRequest = new Request
            {
                Id = requestId,
                UserId = 1,
                Status = true,
                Description = "Test Request",
                CreatedAt = DateTime.UtcNow
            };
            mockRequestRepository.Setup(repo => repo.Get(requestId)).ReturnsAsync(mockRequest);

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.GetRequestById(requestId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockRequest, okResult.Value);
        }
        [TestMethod]
        public async Task GetRequestById_ReturnsNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var requestId = 999; // Non-existing request ID
            mockRequestRepository.Setup(repo => repo.Get(requestId)).ReturnsAsync((Request)null);

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.GetRequestById(requestId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        #endregion
        #region CreateRequest
        [TestMethod]
        public async Task CreateRequest_ReturnsCreated_WhenRequestIsValid()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var requestDto = new RequestDTO
            {
                UserId = 1,
                TypeId = 2,
                Status = true,
                Description = "Test request",
                CreatedAt = DateTime.UtcNow
            };

            // Mock the CreateRequestAsync method to return a new Request object
            var createdRequest = new Request
            {
                Id = 1,
                UserId = 1,
                TypeId = 2,
                Status = true,
                Description = "Test request",
                CreatedAt = DateTime.UtcNow
            };

            mockRequestRepository.Setup(repo => repo.CreateRequestAsync(It.IsAny<RequestDTO>()))
                                 .ReturnsAsync(createdRequest);

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.CreateRequest(requestDto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(nameof(RequestController.GetRequestById), createdResult.ActionName); // Ensure it redirects to GetRequestById
            Assert.AreEqual(createdRequest.Id, createdResult.RouteValues["id"]); // Ensure ID in route matches the created request ID
            Assert.AreEqual(createdRequest, createdResult.Value); // Ensure the response contains the created request
        }

        #endregion
        #region UpdateRequest
        [TestMethod]
        public async Task UpdateRequest_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var requestDto = new RequestDTO
            {
                UserId = 1,
                TypeId = 2,
                Status = true,
                Description = "Updated request description",
                Note = "Updated note",
                CreatedAt = DateTime.UtcNow
            };

            var updatedRequest = new Request
            {
                Id = 1,
                UserId = 1,
                TypeId = 2,
                Status = true,
                Description = "Updated request description",
                Note = "Updated note",
                CreatedAt = requestDto.CreatedAt
            };

            mockRequestRepository.Setup(repo => repo.UpdateRequestAsync(It.IsAny<int>(), It.IsAny<Request>()))
                                 .ReturnsAsync(updatedRequest);

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.UpdateRequest(1, requestDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);  // Status code should be 200
            Assert.AreEqual(updatedRequest, okResult.Value); // Ensure the returned value matches the updated request
        }
        [TestMethod]
        public async Task UpdateRequest_ReturnsBadRequest_WhenIdDoesNotMatchUserId()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var requestDto = new RequestDTO
            {
                UserId = 1,  // This ID does not match the ID in the route
                TypeId = 2,
                Status = true,
                Description = "Request description",
                Note = "Request note",
                CreatedAt = DateTime.UtcNow
            };

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.UpdateRequest(2, requestDto);  // ID in the route is 2, but UserId in DTO is 1

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);  // Status code should be 400 (Bad Request)
        }

        #endregion
        #region DeleteRequest
        [TestMethod]
        public async Task DeleteRequest_ReturnsNoContent_WhenRequestExists()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();
            var request = new Request { Id = 1, UserId = 1, Status = true, Description = "Request description" };

            mockRequestRepository.Setup(repo => repo.Get(It.IsAny<int>())).ReturnsAsync(request); // Simulate the request exists

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.DeleteRequest(1);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);  // Status code should be 204 (No Content)
        }
        [TestMethod]
        public async Task DeleteRequest_ReturnsNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            var mockRequestRepository = new Mock<IRequestRepository>();

            mockRequestRepository.Setup(repo => repo.Get(It.IsAny<int>())).ReturnsAsync((Request)null); // Simulate no request found

            var controller = new RequestController(mockRequestRepository.Object);

            // Act
            var result = await controller.DeleteRequest(1);  // Attempt to delete a non-existing request

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);  // Status code should be 404 (Not Found)
        }

        #endregion
    }
}
