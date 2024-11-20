using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.DTOs.PaymentRentDriver;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentDriverRepository
    {
        Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver();
        Task<bool> AcceptOrDenyRentDriver(int requestId, bool choose);

        Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwner(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwnerId);

    }
}
