using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.ReviewDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class ReviewController_test
    {
        private Mock<IReviewRepository> _mockReviewRepository;
        private Mock<GetInforFromToken> _mockGetInforFromToken;
        private ReviewController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _mockGetInforFromToken = new Mock<GetInforFromToken>();
            _controller = new ReviewController(_mockReviewRepository.Object, _mockGetInforFromToken.Object);
        }
        #region GetAllReviews
        [TestMethod]
        public async Task GetAllReviews_ReturnsOk_WithReviews()
        {
            // Arrange
            var mockReviews = new List<Review>
            {
                new Review { Id = 1, Description = "Great trip!", UserId = 1, TripId = 2 },
                new Review { Id = 2, Description = "Not so great", UserId = 2, TripId = 3 }
            };
            _mockReviewRepository.Setup(repo => repo.GetAll()).ReturnsAsync(mockReviews);

            // Act
            var result = await _controller.GetAllReviews();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockReviews, okResult.Value);
        }

        #endregion
        #region GetReviewById
        [TestMethod]
        public async Task GetReviewById_ReturnsOk_WithReview()
        {
            // Arrange
            var mockReview = new Review { Id = 1, Description = "Great trip!", UserId = 1, TripId = 2 };

            _mockReviewRepository.Setup(repo => repo.Get(1)).ReturnsAsync(mockReview);

            // Act
            var result = await _controller.GetReviewById(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockReview, okResult.Value);
        }

        [TestMethod]
        public async Task GetReviewById_ReturnsNotFound_WhenReviewDoesNotExist()
        {
            // Arrange
            _mockReviewRepository.Setup(repo => repo.Get(It.IsAny<int>())).ReturnsAsync((Review)null);

            // Act
            var result = await _controller.GetReviewById(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        #endregion
        #region CreateReview
        [TestMethod]
        public async Task CreateReview_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var reviewDto = new ReviewDTO(); // Empty or invalid DTO to trigger model validation failure
            _controller.ModelState.AddModelError("Description", "Description is required");

            // Act
            var result = await _controller.CreateReview(reviewDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        #endregion
        #region DeleteReview
        [TestMethod]
        public async Task DeleteReview()
        {
            // Arrange
            var reviewId = 1;
            var review = new Review
            {
                Id = reviewId,
                Description = "Great trip!",
                UserId = 1,
                TripId = 2,
                CreatedAt = DateTime.UtcNow
            };

            _mockReviewRepository.Setup(repo => repo.Get(reviewId)).ReturnsAsync(review);

            // Act
            var result = await _controller.DeleteReview(reviewId);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode); // Status code should be 204 No Content
        }

        // Test case when review does not exist
        [TestMethod]
        public async Task DeleteReview_ReturnsNotFound_WhenReviewDoesNotExist()
        {
            // Arrange
            var reviewId = 1;
            _mockReviewRepository.Setup(repo => repo.Get(reviewId)).ReturnsAsync((Review)null);

            // Act
            var result = await _controller.DeleteReview(reviewId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode); // Status code should be 404 Not Found
        }
        #endregion
    }
}
