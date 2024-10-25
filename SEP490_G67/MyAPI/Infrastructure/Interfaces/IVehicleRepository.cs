using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {

        Task<List<VehicleTypeDTO>> GetVehicleTypeDTOsAsync();

        Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO, string driverName, int roleID);

        Task<bool> UpdateVehicleAsync(int id, string driverName, int userIdUpdate);

        Task<bool> DeleteVehicleAsync(int id);

        Task<List<VehicleListDTO>> GetVehicleDTOsAsync();


    }
}
