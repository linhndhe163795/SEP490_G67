using MyAPI.DTOs.LossCostDTOs.LossCostTypeDTOs;
using MyAPI.Models;
using MyAPI.Repositories.Impls;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ILossCostTypeRepository : IRepository<LossCostType>
    {
        Task<List<LossCostTypeListDTO>> GetAllList();

        Task<LossCostType> CreateLossCostType(LossCostTypeAddDTO lossCostTypeAddDTO);

        Task<bool> UpdateLossCostType(int id, LossCostTypeAddDTO lossCostTypeAddDTO);

        Task<bool> DeleteLossCostType(int id);


    }
}
