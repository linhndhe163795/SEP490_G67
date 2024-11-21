using MyAPI.DTOs.PromotionDTOs;
using MyAPI.DTOs.PromotionUserDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<List<PromotionDTO>> getPromotionUserById(int id);
        Task<PromotionDTO> CreatePromotion(PromotionDTO promotionDTO);
        Task<PromotionDTO> UpdatePromotion(int id, PromotionDTO promotionDTO);

    }
}
