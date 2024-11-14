using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.DTOs.HistoryRentVehicles;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentVehicleRepository : IRepository<HistoryRentVehicle>
    {
        Task<bool> sendMailRequestRentVehicle(string description);

        Task<bool> createVehicleForUser(HistoryVehicleRentDTO historyVehicleDTO);

        Task<List<HistoryRentVehicleListDTO>> historyRentVehicleListDTOs();

        Task<bool> AccpetOrDeninedRentVehicle(int requestId, bool choose);

    }
}
