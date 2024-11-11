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

        public async Task addNewPointUser( int userId)
        {
            try
            {
                var addNewPointUser = new PointUser
                {
                    UserId = userId,
                    Points = 0,
                    PointsMinus = 0,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    Date = DateTime.Now,
                };
                _context.PointUsers.Add(addNewPointUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> addPointUser(PointUserAddDTO pointUserAddDTO)
        {
            try
            {
                var addPoint = new PointUser
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = pointUserAddDTO.CreatedBy,
                    PaymentId = pointUserAddDTO.PaymentId,
                    UpdateBy = pointUserAddDTO.UpdateBy,
                    Points = pointUserAddDTO.Points,
                    PointsMinus = pointUserAddDTO.PointsMinus,
                    Date = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    UserId = pointUserAddDTO.UserId
                };
                await _context.PointUsers.AddAsync(addPoint);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

        public async Task<bool> updatePointUser(int userId, PointUserUpdateDTO pointUserUpdateDTO)
        {
            var checkPoint = await _context.PointUsers.FirstOrDefaultAsync(s => s.UserId == userId);
            if (checkPoint != null)
            {
                checkPoint.Points = checkPoint.Points + pointUserUpdateDTO.Points;
                checkPoint.UpdateAt = DateTime.Now;
                checkPoint.UpdateBy = pointUserUpdateDTO.UpdateBy;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new Exception("Update point user!!!");
            }
        }
    }
}
