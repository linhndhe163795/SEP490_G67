using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PointUserDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PointUserRepository : GenericRepository<PointUser>, IPointUserRepository
    {
        private readonly IMapper _mapper;
        public PointUserRepository(SEP490_G67Context _context, IMapper mapper) : base(_context)
        {
           _mapper = mapper;
        }

        public async Task<PointUserDTOs> getPointUserById(int userId)
        {
            try
            {
                var pointOfUser = await _context.PointUsers.Where(x => x.UserId == userId).ToListAsync();
                var point = pointOfUser.Sum(x => x.Points);
                PointUserDTOs puds = new PointUserDTOs
                {
                    id = 1,
                    Points = point
                };
                return puds;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
