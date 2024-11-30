using AutoMapper;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PromotionDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
    {
        private readonly IMapper _mapper;
        public PromotionRepository(SEP490_G67Context context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<List<PromotionDTO>> getPromotionUserById(int id)
        {
            try
            {
                var listPromotion = await (from pu in _context.PromotionUsers
                                           join p in _context.Promotions on pu.UserId equals p.Id
                                           where p.Id == id
                                           select p).ToListAsync();
                var listPromotionMapper = _mapper.Map<List<PromotionDTO>>(listPromotion);
                return listPromotionMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("getPromotionUserById: " + ex.Message);
            }
        }

        public async Task<PromotionDTO> CreatePromotion(PromotionDTO promotionDTO)
        {
            try
            {
                var promotion = _mapper.Map<Promotion>(promotionDTO);
                promotion.CreatedAt = DateTime.UtcNow;
                promotion.CreatedBy = promotionDTO.CreatedBy;

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();

                return _mapper.Map<PromotionDTO>(promotion);
            }
            catch (Exception ex)
            {
                throw new Exception("CreatePromotion: " + ex.Message);
            }
        }

        public async Task<PromotionDTO> UpdatePromotion(int id, PromotionDTO promotionDTO)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    throw new Exception("Promotion not found");
                }

                promotion.CodePromotion = promotionDTO.CodePromotion;
                promotion.ImagePromotion = promotionDTO.ImagePromotion;
                promotion.Description = promotionDTO.Description;
                promotion.Discount = promotionDTO.Discount;
                promotion.ExchangePoint = promotionDTO.ExchangePoint;
                promotion.StartDate = promotionDTO.StartDate;
                promotion.EndDate = promotionDTO.EndDate;
                promotion.UpdateAt = DateTime.UtcNow;
                promotion.UpdateBy = promotionDTO.UpdateBy;

                _context.Promotions.Update(promotion);
                await _context.SaveChangesAsync();

                return _mapper.Map<PromotionDTO>(promotion);
            }
            catch (Exception ex)
            {
                throw new Exception("UpdatePromotion: " + ex.Message);
            }
        }

        public async Task exchangePromotion(int userId, int promotionId)
        {
            try
            {
                var pointUserId = await _context.PointUsers.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.UserId == userId);
                var promotion = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == promotionId);

                bool checkPromotion = await checkPromtionUserCanChange(promotionId, userId);
                if (!checkPromotion) 
                {
                    throw new Exception("You have promotion !");
                }
                if (pointUserId.Points >= promotion.ExchangePoint)
                {
                    var promotionUser = new PromotionUser
                    {
                        DateReceived = DateTime.UtcNow,
                        PromotionId = promotionId,
                        UserId = userId
                    };
                    _context.PromotionUsers.Add(promotionUser);
                    var pointUserAfterExchange = new PointUser
                    {
                        Points = pointUserId.Points - promotion.ExchangePoint,
                        PointsMinus = promotion.ExchangePoint,
                        PaymentId = null,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdateAt = DateTime.UtcNow,
                        UpdateBy = userId,
                    };
                    _context.PointUsers.Add(pointUserAfterExchange);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Not enough point to exchange");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task<bool> checkPromtionUserCanChange(int promotionId, int userId)
        {
            var listPromtion = await listPromotionCanChange(userId);
            if (listPromtion == null)
            {
                throw new Exception("Not found promotion you can change");
            }
            foreach (var promotion in listPromtion)
            {
                if(promotion.Id == promotionId)
                {
                    return true;
                }
            }
            return false;
        }
    
    public async Task<List<PromotionDTO>> listPromotionCanChange(int userId)
    {
        try
        {
            var userPromotionIds = await _context.PromotionUsers
                                    .Where(pu => pu.UserId == userId)
                                    .Select(pu => pu.PromotionId)
                                    .ToListAsync();
            var listPromotionCanChange = await _context.Promotions
                                    .Where(p => !userPromotionIds.Contains(p.Id)) // Loại bỏ Promotion user đã có
                                    .ToListAsync();

            var mapper = _mapper.Map<List<PromotionDTO>>(listPromotionCanChange);
            return mapper;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
}
