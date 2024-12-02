using MyAPI.DTOs.DriverDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

public interface ITypeOfDriverRepository : IRepository<TypeOfDriver>
{
    Task<TypeOfDriver> CreateTypeOfDriverAsync(UpdateTypeOfDriverDTO updateTypeOfDriverDto);
    Task<TypeOfDriver> UpdateTypeOfDriverAsync(int id, UpdateTypeOfDriverDTO updateTypeOfDriverDto);
}
