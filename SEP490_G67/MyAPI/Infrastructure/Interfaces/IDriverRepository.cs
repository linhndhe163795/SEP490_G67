using MyAPI.DTOs;
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

        Task<IEnumerable<Driver>> GetDriversWithoutVehicleAsync();

        Task SendEmailToDriversWithoutVehicle(int price);

    }
}
