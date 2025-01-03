﻿using MyAPI.DTOs;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Models;


namespace MyAPI.Infrastructure.Interfaces
{
    public interface IDriverRepository : IRepository<Driver>
    {
        Task<int> lastIdDriver();
        Task<Driver> GetDriverWithVehicle(int id);
        Task<Driver> CreateDriverAsync(UpdateDriverDTO updateDriverDto);
        Task<Driver> UpdateDriverAsync(int id, UpdateDriverDTO updateDriverDto);
        Task<IEnumerable<DriverNotVehicleResponse>> GetDriversWithoutVehicleAsync(int vehicleId);
        //Task SendEmailToDriversWithoutVehicle(int price);
        Task<bool> checkLogin(LoginDriverDTO login);
        Task<DriverLoginRespone> getDriverLogin(LoginDriverDTO login);
        Task<List<ListDriverDTO>> getListDriverForVehicle();
        Task BanDriver(int id);
    }
}
