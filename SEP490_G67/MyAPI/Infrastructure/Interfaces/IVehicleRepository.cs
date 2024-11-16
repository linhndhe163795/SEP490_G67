using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {

        Task<List<VehicleTypeDTO>> GetVehicleTypeDTOsAsync();

        Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO, string driverName);

        Task<bool> UpdateVehicleAsync(int id, string driverName);

        Task<bool> DeleteVehicleAsync(int id);

        Task<List<VehicleListDTO>> GetVehicleDTOsAsync();

        Task<bool> AddVehicleByStaffcheckAsync(int requestId, bool isApprove);

        Task<List<EndPointDTO>> GetListEndPointByVehicleId(int vehicleId);
        Task<List<StartPointDTO>> GetListStartPointByVehicleId(int vehicleId);

        Task<bool> AssignDriverToVehicleAsync(int vehicleId, int driverId);
        Task<int> GetNumberSeatAvaiable(int vehicleId);
    }
}
