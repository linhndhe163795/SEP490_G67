using Microsoft.AspNetCore.Http;
using Moq;
using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.DTOs.HistoryRentVehicles;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Test.Test
{
    public class HistoryRentVehicleRepositoryTests
    {
        private readonly Mock<SEP490_G67Context> _contextMock;
        private readonly Mock<SendMail> _sendMailMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<GetInforFromToken> _tokenHelperMock;
        private readonly Mock<IRequestRepository> _requestRepositoryMock;
        private readonly Mock<IRequestDetailRepository> _requestDetailRepositoryMock;
        private readonly HistoryRentVehicleRepository _repository;

        public HistoryRentVehicleRepositoryTests()
        {
            _contextMock = new Mock<SEP490_G67Context>();
            _sendMailMock = new Mock<SendMail>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _tokenHelperMock = new Mock<GetInforFromToken>();
            _requestRepositoryMock = new Mock<IRequestRepository>();
            _requestDetailRepositoryMock = new Mock<IRequestDetailRepository>();

            _repository = new HistoryRentVehicleRepository(
                _contextMock.Object,
                _sendMailMock.Object,
                _httpContextAccessorMock.Object,
                _tokenHelperMock.Object,
                _requestRepositoryMock.Object,
                _requestDetailRepositoryMock.Object,
                null,
                _tokenHelperMock.Object
            );
        }

        [Fact]
        public async Task AccpetOrDeninedRentVehicle_Should_Throw_Exception_When_RequestId_Invalid()
        {
            // Sắp xếp (Arrange)
            var options = new DbContextOptionsBuilder<SEP490_G67Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new SEP490_G67Context(options))
            {
                // Không thêm request nào vào database để kiểm tra trường hợp request không tồn tại
                var repository = new HistoryRentVehicleRepository(
                    context,
                    new Mock<SendMail>().Object,
                    new Mock<IHttpContextAccessor>().Object,
                    new Mock<GetInforFromToken>().Object,
                    new Mock<IRequestRepository>().Object,
                    new Mock<IRequestDetailRepository>().Object,
                    null,
                    new Mock<GetInforFromToken>().Object
                );

                var invalidRequest = new AddHistoryVehicleUseRent { requestId = -1, choose = true, vehicleId = 1, price = 500 };

                // Hành động và Kiểm tra (Act & Assert)
                var ex = await Assert.ThrowsAsync<Exception>(async () =>
                    await repository.AccpetOrDeninedRentVehicle(invalidRequest));

                Assert.Equal("Request not found.", ex.Message);
            }
        }

    }

}
