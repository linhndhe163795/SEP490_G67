﻿using MyAPI.DTOs.VehicleDTOs;
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


    }
}