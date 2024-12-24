using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ILossCostVehicleRepository : IRepository<LossCost>
    {
        Task AddLossCost(LossCostAddDTOs lossCostAddDTOs, int userID);
        Task DeleteLossCost(int id);
        Task<List<AddLostCostVehicleDTOs>> GetAllLostCost();
        Task<TotalLossCost> GetLossCostVehicleByDate(int userId);
        // update 
        Task<TotalLossCost> GetLossCostVehicleByDateUpdate(DateTime? startDate, DateTime? endTime, int? vehicleId, int userId);
        //
        Task UpdateLossCostById(int id, LossCostUpdateDTO lossCostupdateDTOs, int userId);

        //Task AddLossCostVehicle(AddLostCostVehicleDTOs lossCost, int VehicleId);
    }
}
