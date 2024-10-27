using MyAPI.DTOs.PromotionUserDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPromotionUserRepository : IRepository<PromotionUser>
    {
        Task DeletePromotionUser(int id);
        Task AddPromotionAllUser(int promotionId);
    }
}
