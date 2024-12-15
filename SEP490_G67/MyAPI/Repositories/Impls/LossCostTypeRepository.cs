using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.LossCostDTOs.LossCostTypeDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class LossCostTypeRepository : GenericRepository<LossCostType>, ILossCostTypeRepository
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        public LossCostTypeRepository(SEP490_G67Context _context, IMapper mapper, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _mapper = mapper;
        }

        public async Task<LossCostType> CreateLossCostType(LossCostTypeAddDTO lossCostTypeAddDTO)
        {
            if (string.IsNullOrWhiteSpace(lossCostTypeAddDTO.Description))
            {
                throw new Exception("Description cannot be null, empty, or whitespace.");
            }
            var existingLossCostType = await _context.LossCostTypes.FirstOrDefaultAsync(lct => lct.Description == lossCostTypeAddDTO.Description);

            if (existingLossCostType != null)
            {
                throw new Exception("A Loss Cost Type with the same description already exists.");
            }
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User does not exist.");
            }


            var lossCostType = new LossCostType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                Description = lossCostTypeAddDTO.Description,
                UpdateAt = DateTime.Now,
                UpdateBy = userId,
            };
            await _context.LossCostTypes.AddAsync(lossCostType);
            await _context.SaveChangesAsync();
            return lossCostType;
        }

        public async Task<bool> DeleteLossCostType(int id)
        {
            var checkDelete = await _context.LossCostTypes.SingleOrDefaultAsync(x => x.Id == id);
            if (checkDelete == null)
            {
                throw new Exception("Id not found form LossCostType");
            }
            _context.LossCostTypes.Remove(checkDelete);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLossCostType(int id, LossCostTypeAddDTO lossCostTypeAddDTO)
        {
            if (string.IsNullOrWhiteSpace(lossCostTypeAddDTO.Description))
            {
                throw new Exception("Description cannot be null, empty, or whitespace.");
            }
            var existingLossCostType = await _context.LossCostTypes.FirstOrDefaultAsync(lct => lct.Description == lossCostTypeAddDTO.Description);

            if (existingLossCostType != null)
            {
                throw new Exception("A Loss Cost Type with the same description already exists.");
            }
            if (id == null)
            {
                throw new Exception("Id can not be null");
            }

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            var checkUpdate = await _context.LossCostTypes.SingleOrDefaultAsync(x => x.Id == id);
            if (checkUpdate == null)
            {
                throw new Exception("Id not found form LossCostType");
            }

            checkUpdate.Description = lossCostTypeAddDTO.Description;
            checkUpdate.UpdateAt = DateTime.Now;
            checkUpdate.UpdateBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LossCostTypeListDTO>> GetAllList()
        {
            var listLossCostType = await _context.LossCostTypes.OrderByDescending(x => x.Id).ToListAsync();

            var LossCostTypeListDTOs = _mapper.Map<List<LossCostTypeListDTO>>(listLossCostType);

            return LossCostTypeListDTOs;
        }
    }
}
