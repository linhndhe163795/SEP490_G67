using AutoMapper;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PromotionDTOs;
using MyAPI.Helper;
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
                                           join p in _context.Promotions on pu.PromotionId equals p.Id
                                           join u in _context.Users on pu.UserId equals u.Id
                                           where u.Id == id
                                           select p).ToListAsync();
                var listPromotionMapper = _mapper.Map<List<PromotionDTO>>(listPromotion);
                return listPromotionMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("getPromotionUserById: " + ex.Message);
            }
        }

        public async Task<PromotionPost> CreatePromotion(PromotionPost promotionDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(promotionDTO.CodePromotion))
                {
                    throw new ArgumentException("CodePromotion is required and cannot be empty or whitespace.", nameof(promotionDTO.CodePromotion));
                }

                if (string.IsNullOrWhiteSpace(promotionDTO.Description))
                {
                    throw new ArgumentException("Description is required and cannot be empty or whitespace.", nameof(promotionDTO.Description));
                }

                if (promotionDTO.Discount <= 0)
                {
                    throw new ArgumentException("Discount must be greater than zero.", nameof(promotionDTO.Discount));
                }

                if (promotionDTO.ExchangePoint.HasValue && promotionDTO.ExchangePoint.Value <= 0)
                {
                    throw new ArgumentException("ExchangePoint must be greater than zero if provided.", nameof(promotionDTO.ExchangePoint));
                }

                if (promotionDTO.StartDate.HasValue && promotionDTO.EndDate.HasValue && promotionDTO.StartDate > promotionDTO.EndDate)
                {
                    throw new ArgumentException("StartDate must be earlier than or equal to EndDate.", nameof(promotionDTO.StartDate));
                }
                var existingPromotion = await _context.Promotions
                                    .FirstOrDefaultAsync(p => p.CodePromotion == promotionDTO.CodePromotion);

                if (existingPromotion != null)
                {
                    throw new Exception("Promotion code already exists.");
                }
                var promotion = _mapper.Map<Promotion>(promotionDTO);
                promotion.CreatedAt = DateTime.UtcNow;
                promotion.CreatedBy = Constant.STAFF;

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();

                return _mapper.Map<PromotionPost>(promotion);
            }
            catch (Exception ex)
            {
                throw new Exception("CreatePromotion: " + ex.Message);
            }
        }

        public async Task<PromotionPost> UpdatePromotion(int id, PromotionPost promotionDTO)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    throw new Exception("Promotion not found");
                }
                if (string.IsNullOrWhiteSpace(promotionDTO.CodePromotion))
                {
                    throw new ArgumentException("CodePromotion is required and cannot be empty or whitespace.", nameof(promotionDTO.CodePromotion));
                }

                if (string.IsNullOrWhiteSpace(promotionDTO.Description))
                {
                    throw new ArgumentException("Description is required and cannot be empty or whitespace.", nameof(promotionDTO.Description));
                }

                if (promotionDTO.Discount <= 0 || promotionDTO.Discount > 100)
                {
                    throw new ArgumentException("Discount must be between 1 and 100.", nameof(promotionDTO.Discount));
                }

                if (promotionDTO.ExchangePoint.HasValue && promotionDTO.ExchangePoint.Value <= 0)
                {
                    throw new ArgumentException("ExchangePoint must be greater than zero if provided.", nameof(promotionDTO.ExchangePoint));
                }

                if (promotionDTO.StartDate.HasValue && promotionDTO.EndDate.HasValue && promotionDTO.StartDate >= promotionDTO.EndDate)
                {
                    throw new ArgumentException("StartDate must be earlier than EndDate.", nameof(promotionDTO.StartDate));
                }
                if (promotionDTO.StartDate >= promotionDTO.EndDate)
                {
                    throw new Exception("Start date must be earlier than end date.");
                }

                if (promotionDTO.Discount <= 0 || promotionDTO.Discount > 100)
                {
                    throw new Exception("Discount must be between 1 and 100.");
                }
                if (promotionDTO.Discount <= 0 || promotionDTO.Discount > 100)
                {
                    throw new Exception("Discount must be between 1 and 100.");
                }

                var existingPromotion = await _context.Promotions
                                        .FirstOrDefaultAsync(p => p.CodePromotion == promotionDTO.CodePromotion);

                if (existingPromotion != null)
                {
                    throw new Exception("Promotion code already exists.");
                }

                promotion.CodePromotion = promotionDTO.CodePromotion;
                promotion.ImagePromotion = promotionDTO.ImagePromotion;
                promotion.Description = promotionDTO.Description;
                promotion.Discount = promotionDTO.Discount;
                promotion.ExchangePoint = promotionDTO.ExchangePoint;
                promotion.StartDate = promotionDTO.StartDate;
                promotion.EndDate = promotionDTO.EndDate;
                promotion.UpdateAt = DateTime.UtcNow;
                promotion.UpdateBy = Constant.STAFF;

                _context.Promotions.Update(promotion);
                await _context.SaveChangesAsync();

                return _mapper.Map<PromotionPost>(promotion);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task exchangePromotion(int userId, int promotionId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
                }

                if (promotionId <= 0)
                {
                    throw new ArgumentException("PromotionId must be greater than zero.", nameof(promotionId));
                }
                var pointUserId = await _context.PointUsers.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.UserId == userId);
                var promotion = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == promotionId);
                if (promotion == null)
                {
                    throw new Exception("Promotion not found.");
                }
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
                        UserId = userId,
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
                if (promotion.Id == promotionId)
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

        public async Task<PromotionDTO> getPromotionByPromotionId(int promotionId)
        {
            try
            {
                var promotionById = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == promotionId);
                if (promotionById == null)
                {
                    throw new Exception("Not Found Promotion");
                }
                var mapper = _mapper.Map<PromotionDTO>(promotionById);
                return mapper;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
