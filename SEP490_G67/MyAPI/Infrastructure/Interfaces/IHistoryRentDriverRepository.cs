using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.DTOs.PaymentRentDriver;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentDriverRepository : IRepository<HistoryRentDriver>
    {
        Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver();
        Task<bool> AcceptOrDenyRentDriver(AddHistoryRentDriver add);

        Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwner();
        Task<bool> UpdateDriverInRequestAsync(int driverId, int requestId);
        Task<List<DriverHistoryDTO>> getHistoryRentDriver(int userId, string role);
        // update version 2
        Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwnerUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId);
        Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriverUpdate(int requestDeatailsId);


    }
}
