using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Models;


namespace MyAPI.Infrastructure.Interfaces
{
    public interface IDriverRepository : IRepository<Driver>
{
        Task<int> lastIdDriver();
        Task<Driver> GetDriverWithVehicle(int id);
}
}
