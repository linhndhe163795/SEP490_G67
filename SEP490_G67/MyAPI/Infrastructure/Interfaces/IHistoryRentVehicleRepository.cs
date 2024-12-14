using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.DTOs.HistoryRentVehicles;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentVehicleRepository : IRepository<HistoryRentVehicle>
    {
        Task<bool> sendMailRequestRentVehicle(string description);

        Task<bool> createVehicleForUser(HistoryVehicleRentDTO historyVehicleDTO);

        Task<List<Vehicle>> historyRentVehicleListDTOs(DateTime dateTime);

        Task<bool> AccpetOrDeninedRentVehicle(AddHistoryVehicleUseRent add);
        Task<List<HistoryVehicleRentDTO>> listHistoryRentVehicle(int userId, string roleName);
    }
}
