﻿using MyAPI.DTOs.PromotionUserDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPromotionUserRepository : IRepository<PromotionUser>
    {
        Task DeletePromotionUser(int id);
        Task AddPromotionAllUser(int promotionId);

        Task<bool> DeletePromotionAfterPayment(int userId, int promotion_id);
    }
}
