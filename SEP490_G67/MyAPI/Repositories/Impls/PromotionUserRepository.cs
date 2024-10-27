using AutoMapper;
using MyAPI.DTOs.PromotionUserDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Collections;

namespace MyAPI.Repositories.Impls
{
    public class PromotionUserRepository : GenericRepository<PromotionUser>, IPromotionUserRepository
    {
        private readonly IMapper _mapper;
        public PromotionUserRepository(SEP490_G67Context _context, IMapper mapper) : base(_context)
        {
            _mapper = mapper;
        }

        public async Task AddPromotionAllUser(int promotionId)
        {
            try
            {
                var listUser = _context.Users.ToList();
                List<PromotionUserDTO> listPromotionUser = new List<PromotionUserDTO>();
                foreach (var user in listUser) 
                {
                    listPromotionUser.Add(new PromotionUserDTO
                    {
                        UserId = user.Id,
                        PromotionId = promotionId,
                        DateReceived = DateTime.Now,
                    });
                }
                var listPromotionUserMapper = _mapper.Map<List<PromotionUser>>(listPromotionUser);
                await _context.AddRangeAsync(listPromotionUserMapper);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                throw new Exception("AddPromotionAllUser: " + ex.Message);
            }
        }

        public async Task DeletePromotionUser(int id)
        {
            try
            {
                List<PromotionUser> promotionUser = new List<PromotionUser>();
                var listPromotionUser = _context.PromotionUsers.Where(x => x.PromotionId == id).ToList();
                foreach (var item in listPromotionUser)
                {
                    promotionUser.Add(item);
                }
                _context.RemoveRange(promotionUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("DeletePromotionUser " + ex.Message);
            }

        }
    }
}
