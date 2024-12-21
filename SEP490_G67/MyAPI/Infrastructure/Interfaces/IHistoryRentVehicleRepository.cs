using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.DTOs.HistoryRentVehicles;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IHistoryRentVehicleRepository : IRepository<HistoryRentVehicle>
    {
        Task<bool> sendMailRequestRentVehicle(string description);

        Task<bool> createVehicleForUser(HistoryVehicleRentDTO historyVehicleDTO);

        Task<List<VehicleConvenienceRentResponseDTO>> historyRentVehicleListDTOs(DateTime dateTime);

        Task<bool> AccpetOrDeninedRentVehicle(AddHistoryVehicleUseRent add);
        Task<List<HistoryVehicleRentDTO>> listHistoryRentVehicle(int userId, string roleName);
        Task<List<Vehicle>> GetAvailableVehicles(int requestId);
    }
}
