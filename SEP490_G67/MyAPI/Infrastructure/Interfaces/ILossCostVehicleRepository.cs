using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ILossCostVehicleRepository : IRepository<LossCost>
    {
        Task<List<AddLostCostVehicleDTOs>> GetAllLostCost();
        Task<TotalLossCost> GetLossCostVehicleByDate(int? vehicleId, DateTime? startDate, DateTime? endDate);
        
        //Task AddLossCostVehicle(AddLostCostVehicleDTOs lossCost, int VehicleId);
    }
}
