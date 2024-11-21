using AutoMapper;
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
    }
}
