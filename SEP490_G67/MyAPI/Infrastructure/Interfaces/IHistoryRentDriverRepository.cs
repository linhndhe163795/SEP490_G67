using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.DTOs.PaymentRentDriver;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentDriverRepository : IRepository<HistoryRentDriver>
    {
        Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver();
        Task<bool> AcceptOrDenyRentDriver(AddHistoryRentDriver add);

        Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwner(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwnerId);
        Task<bool> UpdateDriverInRequestAsync(int driverId, int requestId);
        Task<List<DriverHistoryDTO>> getHistoryRentDriver(int userId, string role);
    }
}
