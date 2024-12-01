using MyAPI.DTOs.PromotionDTOs;
using MyAPI.DTOs.PromotionUserDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<List<PromotionDTO>> getPromotionUserById(int id);
        Task<PromotionPost> CreatePromotion(PromotionPost promotionDTO);
        Task<PromotionPost> UpdatePromotion(int id, PromotionPost promotionDTO);
        Task exchangePromotion(int userId, int promotionId);
        Task<List<PromotionDTO>> listPromotionCanChange(int userId);
    }
}
