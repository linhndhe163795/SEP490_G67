using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyAPI.Controllers;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Test
{
    [TestClass]
    public class TicketController_Test
    {
        private Mock<ITicketRepository> _mockTicketRepository;
        private Mock<GetInforFromToken> _mockGetInforFromToken;
        private Mock<IMapper> _mockMapper;
        private TicketController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockTicketRepository = new Mock<ITicketRepository>();
            _mockGetInforFromToken = new Mock<GetInforFromToken>();
            _mockMapper = new Mock<IMapper>();

            _controller = new TicketController(
                _mockTicketRepository.Object,
                _mockGetInforFromToken.Object,
                _mockMapper.Object
            );
        }
        [TestMethod]
        public async Task GetListTicket_ReturnsOk_WhenTicketsExist()
        {
            // Arrange
            var mockTickets = new List<ListTicketDTOs> { new ListTicketDTOs { Price = 100 } };
            _mockTicketRepository.Setup(repo => repo.getAllTicket()).ReturnsAsync(mockTickets);
            _mockMapper.Setup(m => m.Map<List<ListTicketDTOs>>(mockTickets)).Returns(new List<ListTicketDTOs>());

            // Act
            var result = await _controller.getListTicket();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(List<ListTicketDTOs>));
        }
        [TestMethod]
        public async Task GetListTicket_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var exceptionMessage = "Database error";
            _mockTicketRepository.Setup(repo => repo.getAllTicket()).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.getListTicket();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual($"getListTicket: {exceptionMessage}", badRequestResult.Value);
        }
        [TestMethod]
        public async Task GetListTicketNotPaid_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            int vehicleId = 1; // Valid vehicleId
            _mockTicketRepository
                .Setup(repo => repo.GetListTicketNotPaid(vehicleId))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _controller.getListTicketNotPaid(vehicleId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsTrue(badRequestResult.Value.ToString().Contains("getListTicket: Database error"));
        }
        [TestMethod]
        public async Task GetListTicketNotPaid_ReturnsOk_WhenTicketsExist()
        {
            // Arrange
            int vehicleId = 1; // Example valid vehicleId
            var ticketsNotPaid = new List<TicketNotPaid> // Sample data returned by the repository
            {
                new TicketNotPaid { FullName = "NVA", Price = 100.0m, TypeOfPayment = "Credit" },
                new TicketNotPaid { FullName = "NVB", Price = 150.0m, TypeOfPayment = "Cash" }
            };

            // Mock repository to return ticketsNotPaid
            _mockTicketRepository
                .Setup(repo => repo.GetListTicketNotPaid(vehicleId))
                .ReturnsAsync(ticketsNotPaid);

            var mappedTickets = new List<ListTicketDTOs>
            {
                new ListTicketDTOs { Price = 100.0m, TypeOfPayment = 1 },
                new ListTicketDTOs { Price = 150.0m, TypeOfPayment = 2 }
            };

            // Mock the mapper to convert TicketNotPaid to ListTicketDTOs
            _mockMapper
                .Setup(mapper => mapper.Map<List<ListTicketDTOs>>(ticketsNotPaid))
                .Returns(mappedTickets);

            // Act
            var result = await _controller.getListTicketNotPaid(vehicleId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }
        
    }
}
