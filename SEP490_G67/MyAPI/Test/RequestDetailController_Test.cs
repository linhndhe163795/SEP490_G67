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
    public class RequestDetailController_Test
    {
        private Mock<IRequestDetailRepository> _mockRequestDetailRepository;
        private RequestDetailController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRequestDetailRepository = new Mock<IRequestDetailRepository>();
            _controller = new RequestDetailController(_mockRequestDetailRepository.Object);
        }

        #region GetAllRequestDetails
        [TestMethod]
        public async Task GetAllRequestDetails_ReturnsOk_WhenRequestDetailsExist()
        {
            // Arrange
            var requestDetails = new List<RequestDetail>
            {
                new RequestDetail { DetailId = 1, RequestId = 1, VehicleId = 1, StartLocation = "Location A", EndLocation = "Location B" },
                new RequestDetail { DetailId = 2, RequestId = 2, VehicleId = 2, StartLocation = "Location C", EndLocation = "Location D" }
            };

            _mockRequestDetailRepository.Setup(repo => repo.GetAll()).ReturnsAsync(requestDetails);

            // Act
            var result = await _controller.GetAllRequestDetails();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); // Status code should be OK
            var returnValue = okResult.Value as List<RequestDetail>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count); // Verify that the correct number of items is returned
        }
        [TestMethod]
        public async Task GetAllRequestDetails_ReturnsOk_EmptyList_WhenNoRequestDetailsExist()
        {
            // Arrange
            var requestDetails = new List<RequestDetail>(); // Empty list

            _mockRequestDetailRepository.Setup(repo => repo.GetAll()).ReturnsAsync(requestDetails);

            // Act
            var result = await _controller.GetAllRequestDetails();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); // Status code should be OK
            var returnValue = okResult.Value as List<RequestDetail>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(0, returnValue.Count); // Verify that the returned list is empty
        }

        #endregion
        #region GetRequestDetailById
        [TestMethod]
        public async Task GetRequestDetailById_ReturnsOk_WhenRequestDetailExists()
        {
            // Arrange
            var requestDetail = new RequestDetail { DetailId = 1, RequestId = 1, VehicleId = 1, StartLocation = "Location A", EndLocation = "Location B" };

            _mockRequestDetailRepository.Setup(repo => repo.Get(1)).ReturnsAsync(requestDetail);

            // Act
            var result = await _controller.GetRequestDetailById(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); // Status code should be OK
            var returnValue = okResult.Value as RequestDetail;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(1, returnValue.DetailId); // Check if the correct request detail is returned
        }
        [TestMethod]
        public async Task GetRequestDetailById_ReturnsNotFound_WhenRequestDetailDoesNotExist()
        {
            // Arrange
            _mockRequestDetailRepository.Setup(repo => repo.Get(1)).ReturnsAsync((RequestDetail)null);

            // Act
            var result = await _controller.GetRequestDetailById(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode); // Status code should be 404 for Not Found
        }

        #endregion
        #region CreateRequestDetail

        [TestMethod]
        public async Task CreateRequestDetail_ReturnsCreatedAtAction_WhenRequestDetailIsCreated()
        {
            // Arrange
            var requestDetailDto = new RequestDetailDTO
            {
                RequestId = 1,
                VehicleId = 1,
                StartLocation = "Location A",
                EndLocation = "Location B",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Seats = 4
            };

            var createdRequestDetail = new RequestDetail
            {
                DetailId = 1,
                RequestId = 1,
                VehicleId = 1,
                StartLocation = "Location A",
                EndLocation = "Location B",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Seats = 4
            };

            _mockRequestDetailRepository.Setup(repo => repo.CreateRequestDetailAsync(It.IsAny<RequestDetailDTO>()))
                                        .ReturnsAsync(createdRequestDetail);

            // Act
            var result = await _controller.CreateRequestDetail(requestDetailDto);

            // Assert
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(201, createdAtActionResult.StatusCode); // Status code should be 201 Created

            var returnValue = createdAtActionResult.Value as RequestDetail;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(createdRequestDetail.DetailId, returnValue.DetailId); // Ensure the created entity is returned correctly
            Assert.AreEqual(createdRequestDetail.RequestId, returnValue.RequestId);
            Assert.AreEqual(createdRequestDetail.VehicleId, returnValue.VehicleId);
            Assert.AreEqual(createdRequestDetail.StartLocation, returnValue.StartLocation);
            Assert.AreEqual(createdRequestDetail.EndLocation, returnValue.EndLocation);
        }

        #endregion
        #region UpdateRequestDetail
        [TestMethod]
        public async Task UpdateRequestDetail_ReturnsOk_WhenRequestDetailIsUpdated()
        {
            // Arrange
            var requestDetailDto = new RequestDetailDTO
            {
                RequestId = 1,
                VehicleId = 1,
                StartLocation = "Location A",
                EndLocation = "Location B",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Seats = 4
            };

            var updatedRequestDetail = new RequestDetail
            {
                DetailId = 1,
                RequestId = 1,
                VehicleId = 1,
                StartLocation = "Location A",
                EndLocation = "Location B",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Seats = 4
            };

            // Mock the repository method to return the updated request detail
            _mockRequestDetailRepository.Setup(repo => repo.UpdateRequestDetailAsync(It.IsAny<int>(), It.IsAny<RequestDetailDTO>()))
                                        .ReturnsAsync(updatedRequestDetail);

            // Act
            var result = await _controller.UpdateRequestDetail(1, requestDetailDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); // Status code should be 200 OK

            var returnValue = okResult.Value as RequestDetail;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(updatedRequestDetail.DetailId, returnValue.DetailId); // Ensure the updated entity is returned correctly
            Assert.AreEqual(updatedRequestDetail.RequestId, returnValue.RequestId);
            Assert.AreEqual(updatedRequestDetail.VehicleId, returnValue.VehicleId);
            Assert.AreEqual(updatedRequestDetail.StartLocation, returnValue.StartLocation);
            Assert.AreEqual(updatedRequestDetail.EndLocation, returnValue.EndLocation);
        }
        [TestMethod]
        public async Task UpdateRequestDetail_ReturnsBadRequest_WhenIdDoesNotMatchRequestId()
        {
            // Arrange
            var requestDetailDto = new RequestDetailDTO
            {
                RequestId = 2, // RequestId is 2, but we are passing id=1 in the URL
                VehicleId = 1,
                StartLocation = "Location A",
                EndLocation = "Location B",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Seats = 4
            };

            // Act
            var result = await _controller.UpdateRequestDetail(1, requestDetailDto);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // Status code should be 400 Bad Request
        }

        #endregion
        #region DeleteRequestDetail
        [TestMethod]
        public async Task DeleteRequestDetail_ReturnsNoContent_WhenRequestDetailIsDeleted()
        {
            // Arrange
            var requestDetail = new RequestDetail
            {
                DetailId = 1,
                RequestId = 1,
                VehicleId = 1,
                StartLocation = "Location A",
                EndLocation = "Location B",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Seats = 4
            };

            _mockRequestDetailRepository.Setup(repo => repo.Get(It.IsAny<int>())).ReturnsAsync(requestDetail);
            // Act
            var result = await _controller.DeleteRequestDetail(1);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode); // Status code should be 204 No Content
        }
        [TestMethod]
        public async Task DeleteRequestDetail_ReturnsNotFound_WhenRequestDetailDoesNotExist()
        {
            // Arrange
            _mockRequestDetailRepository.Setup(repo => repo.Get(It.IsAny<int>())).ReturnsAsync((RequestDetail)null);

            // Act
            var result = await _controller.DeleteRequestDetail(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode); // Status code should be 404 Not Found
        }
        #endregion
    }
}
