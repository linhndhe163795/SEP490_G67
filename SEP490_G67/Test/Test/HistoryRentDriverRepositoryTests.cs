using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Test
{
    public class HistoryRentDriverRepositoryTests
    {
        private readonly Mock<SEP490_G67Context> _contextMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<GetInforFromToken> _tokenHelperMock;
        private readonly Mock<IRequestRepository> _requestRepositoryMock;
        private readonly Mock<IRequestDetailRepository> _requestDetailRepositoryMock;
        private readonly HistoryRentDriverRepository _repository;

        public HistoryRentDriverRepositoryTests()
        {
            _contextMock = new Mock<SEP490_G67Context>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _tokenHelperMock = new Mock<GetInforFromToken>();
            _requestRepositoryMock = new Mock<IRequestRepository>();
            _requestDetailRepositoryMock = new Mock<IRequestDetailRepository>();

            _repository = new HistoryRentDriverRepository(
                _contextMock.Object,
                _httpContextAccessorMock.Object,
                _tokenHelperMock.Object,
                _requestRepositoryMock.Object,
                _requestDetailRepositoryMock.Object
            );
        }

        [Fact]
        public async Task AcceptOrDenyRentDriver_Should_Throw_Exception_When_RequestId_Invalid()
        {
            // Arrange
            var invalidRequest = new AddHistoryRentDriver { requestId = -1, choose = true, driverId = 1, price = 100 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await _repository.AcceptOrDenyRentDriver(invalidRequest));

            Assert.Equal("Invalid request ID.", ex.Message);
        }

        [Fact]
        public async Task AcceptOrDenyRentDriver_Should_Throw_Exception_When_DriverId_Is_Null()
        {
            // Arrange
            var invalidRequest = new AddHistoryRentDriver { requestId = 1, choose = true, driverId = null, price = 100 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await _repository.AcceptOrDenyRentDriver(invalidRequest));

            Assert.Equal("Driver ID cannot be null.", ex.Message);
        }

    }
}
