using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.DTOs.PaymentRentDriver;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentDriverRepository
    {
        Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver();
        Task<bool> AcceptOrDenyRentDriver(AddHistoryRentDriver add);

        Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwner(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwnerId);
        Task<bool> UpdateDriverInRequestAsync(int driverId, int requestId);

        Task<List<DriverHistoryDTO>> GetDriverHistoryByUserIdAsync();
        Task<List<DriverRentInfoDTO>> GetDriverRentInfo(DateTime? startDate, DateTime? endDate);
        Task<List<DriverHistoryDTO>> GetHistoryByVehicleOwnerAsync();

    }
}
