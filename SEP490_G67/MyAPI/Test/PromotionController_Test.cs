using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.PromotionDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class PromotionController_Test
    {
        private Mock<IPromotionRepository> promotionRepositoryMock;
        private Mock<IPromotionUserRepository> promotionUserRepositoryMock;
        private Mock<GetInforFromToken> getInforFromTokenMock;
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IMapper> mapperMock;

        private PromotionController controller;

        [TestInitialize]
        public void Setup()
        {
            // Create mocks for all dependencies
            promotionRepositoryMock = new Mock<IPromotionRepository>();
            promotionUserRepositoryMock = new Mock<IPromotionUserRepository>();
            getInforFromTokenMock = new Mock<GetInforFromToken>();
            userRepositoryMock = new Mock<IUserRepository>();
            mapperMock = new Mock<IMapper>();

            // Instantiate the controller with mocked dependencies
            controller = new PromotionController(
                promotionRepositoryMock.Object,
                getInforFromTokenMock.Object,
                mapperMock.Object,
                promotionUserRepositoryMock.Object,
                userRepositoryMock.Object
            );
        }
        #region GetAllPromotion
        [TestMethod]
        public async Task GetAllPromotion_ReturnsOk_WithMappedPromotions()
        {
            // Arrange
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = 1,
                    CodePromotion = "PROMO2024",
                    ImagePromotion = "https://example.com/promo1.jpg",
                    Discount = 10,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddMonths(1),
                    Description = "10% off on all items",
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdateAt = DateTime.Now,
                    UpdateBy = 1
                },
                new Promotion
                {
                    Id = 2,
                    CodePromotion = "PROMO2024",
                    ImagePromotion = "https://example.com/promo1.jpg",
                    Discount = 10,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddMonths(1),
                    Description = "10% off on all items",
                    CreatedAt = DateTime.Now,
                    CreatedBy = 2,
                    UpdateAt = DateTime.Now,
                    UpdateBy = 2
                },
            };

            var promotionDTOs = new List<PromotionDTO>
            {
                new PromotionDTO
                {
                    CodePromotion = "PROMO2024",
                    ImagePromotion = "https://example.com/promo1.jpg",
                    Discount = 10,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddMonths(1),
                    Description = "10% off on all items",
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdateAt = DateTime.Now,
                    UpdateBy = 1
                },
                new PromotionDTO
                {
                    CodePromotion = "PROMO2025",
                    ImagePromotion = "https://example.com/promo2.jpg",
                    Discount = 15,
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddMonths(2),
                    Description = "15% off on selected items",
                    CreatedAt = DateTime.Now,
                    CreatedBy = 2,
                    UpdateAt = DateTime.Now,
                    UpdateBy = 2
                }
            };

            // Setup mocks
            promotionRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(promotions);
            mapperMock.Setup(mapper => mapper.Map<List<PromotionDTO>>(promotions)).Returns(promotionDTOs);

            // Act
            var result = await controller.GetAllPromotion();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(promotionDTOs, okResult.Value);

            promotionRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<List<PromotionDTO>>(promotions), Times.Once);
        }

        [TestMethod]
        public async Task GetAllPromotion_ReturnsBadRequest_OnException()
        {
            // Arrange
            promotionRepositoryMock.Setup(repo => repo.GetAll()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetAllPromotion();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Database error", badRequestResult.Value);

            promotionRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()), Times.Never);
        }

        #endregion
        #region updatePromotion
        [TestMethod]
        public async Task UpdatePromotion_ReturnsOk_WhenPromotionUpdatedSuccessfully()
        {
            // Arrange
            var promotionDTO = new PromotionDTO
            {
                Description = "New Description",
                Discount = 20,
                CodePromotion = "NEWCODE",
                ImagePromotion = "new_image.jpg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            var promotion = new Promotion
            {
                Id = 1,
                Description = "Old Description",
                Discount = 10,
                CodePromotion = "OLDCODE",
                ImagePromotion = "old_image.jpg",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(1),
                UpdateAt = DateTime.Now,
                UpdateBy = 1
            };

            // Mock the Get method to return an existing promotion
            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync(promotion);

            // Act
            var result = await controller.UpdatePromotion(1, promotionDTO, null);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(promotionDTO, okResult.Value);
        }

        [TestMethod]
        public async Task UpdatePromotion_ReturnsNotFound_WhenPromotionDoesNotExist()
        {
            // Arrange
            var promotionDTO = new PromotionDTO
            {
                Description = "New Description",
                Discount = 20,
                CodePromotion = "NEWCODE",
                ImagePromotion = "new_image.jpg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            // Mock the Get method to return null (promotion not found)
            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync((Promotion)null);

            // Act
            var result = await controller.UpdatePromotion(1, promotionDTO, null);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found promotion had id = 1", notFoundResult.Value);
        }

        [TestMethod]
        public async Task UpdatePromotion_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var promotionDTO = new PromotionDTO
            {
                Description = "New Description",
                Discount = 20,
                CodePromotion = "NEWCODE",
                ImagePromotion = "new_image.jpg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            // Mock the Get method to throw an exception
            promotionRepositoryMock.Setup(repo => repo.Get(It.IsAny<int>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.UpdatePromotion(1, promotionDTO, null);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Database error", badRequestResult.Value);
        }
        #endregion
        #region deletePromotion
        [TestMethod]
        public async Task DeletePromotion_ReturnsOk_WhenPromotionDeletedSuccessfully()
        {
            // Arrange
            var promotion = new Promotion { Id = 1, Description = "Old Description" };

            // Mock the Get method to return a promotion
            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync(promotion);

            promotionUserRepositoryMock.Setup(repo => repo.DeletePromotionUser(1)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeletePromotion(1);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task DeletePromotion_ReturnsNotFound_WhenPromotionDoesNotExist()
        {
            // Arrange
            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync((Promotion)null);

            // Act
            var result = await controller.DeletePromotion(1);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found promotion had id = 1", notFoundResult.Value);
        }

        [TestMethod]
        public async Task DeletePromotion_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            promotionRepositoryMock.Setup(repo => repo.Get(It.IsAny<int>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.DeletePromotion(1);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Database error", badRequestResult.Value);
        }

        [TestMethod]
        public async Task DeletePromotion_ReturnsBadRequest_WhenDeleteFails()
        {
            // Arrange
            var promotion = new Promotion { Id = 1, Description = "Old Description" };

            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync(promotion);

            promotionRepositoryMock.Setup(repo => repo.Delete(It.IsAny<Promotion>())).ThrowsAsync(new Exception("Failed to delete promotion"));

            // Act
            var result = await controller.DeletePromotion(1);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Failed to delete promotion", badRequestResult.Value);
        }
        #endregion
        #region GivePromotionToUser
        [TestMethod]
        public async Task GivePromotionToUser_ReturnsOk_WhenPromotionAndUserExist()
        {
            // Arrange
            var promotion = new Promotion { Id = 1, Description = "Special Offer" };
            var user = new User { Id = 1, FullName = "Nguyễn Văn A" };

            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync(promotion);
            userRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync(user);


            // Act
            var result = await controller.GivePromotionToUser(1, 1);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task GivePromotionToUser_ReturnsNotFound_WhenPromotionDoesNotExist()
        {
            // Arrange
            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync((Promotion)null);

            // Act
            var result = await controller.GivePromotionToUser(1, 1);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found promotion had id = 1", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GivePromotionToUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var promotion = new Promotion { Id = 1, Description = "Special Offer" };

            promotionRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync(promotion);
            userRepositoryMock.Setup(repo => repo.Get(1)).ReturnsAsync((User)null);

            // Act
            var result = await controller.GivePromotionToUser(1, 1);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Not found user had id = 1", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GivePromotionToUser_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            promotionRepositoryMock.Setup(repo => repo.Get(It.IsAny<int>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GivePromotionToUser(1, 1);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Database error", badRequestResult.Value);
        }
        #endregion
        #region GivePromotionAllUser
        [TestMethod]
        public async Task GivePromotionAllUser_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var promotionDTO = new PromotionDTO { Description = "Special Offer", Discount = 20 };
            var promotion = new Promotion { Id = 1, Description = "Special Offer", Discount = 20 };

            mapperMock.Setup(m => m.Map<Promotion>(promotionDTO)).Returns(promotion);

            // Act
            var result = await controller.GivePromotionAllUser(promotionDTO);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(promotionDTO, okResult.Value);
        }

        [TestMethod]
        public async Task GivePromotionAllUser_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var promotionDTO = new PromotionDTO { Description = "Special Offer", Discount = 20 };

            // Mock the mapping to return a Promotion object
            mapperMock.Setup(m => m.Map<Promotion>(promotionDTO)).Returns(new Promotion());

            // Mock the Add method to throw an exception
            promotionRepositoryMock.Setup(repo => repo.Add(It.IsAny<Promotion>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GivePromotionAllUser(promotionDTO);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("GivePromotionAllUser: Database error", badRequestResult.Value);
        }
        #endregion
    }
}
