using MyAPI.DTOs.HistoryRentDriverDTOs;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentDriverRepository
    {
        Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver();
        Task<bool> AcceptOrDenyRentDriver(int requestId, bool choose);
    }
}
