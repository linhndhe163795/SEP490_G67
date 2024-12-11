using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {

        Task<List<VehicleTypeDTO>> GetVehicleTypeDTOsAsync();
        Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO);
        Task<bool> UpdateVehicleAsync(int id, string driverName);
        Task<bool> DeleteVehicleAsync(int id);
        Task<List<VehicleListDTO>> GetVehicleDTOsAsync(int userId, string role);
        Task<bool> AddVehicleByStaffcheckAsync(int requestId, bool isApprove);
        Task<List<EndPointDTO>> GetListEndPointByVehicleId(int vehicleId, string startPoint);
        Task<List<StartPointDTO>> GetListStartPointByVehicleId(int vehicleId);
        Task<bool> AssignDriverToVehicleAsync(int vehicleId, int driverId);
        Task<int> GetNumberSeatAvaiable(int tripId, DateTime dateTime);
        Task<VehicleAddDTO> GetVehicleById(int vehicleId);
        Task<List<VehicleLicenscePlateDTOs>> getLicensecePlate();
        Task<List<VehicleLicenscePlateDTOs>> getVehicleByDriverId(int driverId);
        Task<bool> checkDriver(int vehicleId,int driverId);
        Task<bool> UpdateVehicleAsync(int id, VehicleUpdateDTO updateDTO);
        Task<List<VehicleLicenscePlateDTOs>> getVehicleByVehicleOwner(int vehicleOwner);
        Task<(bool IsSuccess, List<ValidationErrorDTO> Errors)> ConfirmAddValidEntryImportVehicle(List<VehicleImportDTO> validEntries);
        Task<List<VehicleBasicDto>> GetAvailableVehiclesAsync();
    }
}
