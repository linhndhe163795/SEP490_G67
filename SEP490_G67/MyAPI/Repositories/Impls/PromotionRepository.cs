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
    }
}
