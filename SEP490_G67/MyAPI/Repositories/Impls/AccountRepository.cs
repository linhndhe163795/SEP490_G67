using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Diagnostics.Eventing.Reader;

namespace MyAPI.Repositories.Impls
{
    public class AccountRepository : GenericRepository<User>, IAccountRepository 
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GetInforFromToken _inforFromToken;


        public AccountRepository(SEP490_G67Context _context,  IMapper mapper, 
            IHttpContextAccessor httpContextAccessor, GetInforFromToken inforFromToken) : base(_context)
        {
            _contextAccessor = httpContextAccessor;
            _inforFromToken = inforFromToken;
            _mapper = mapper;
        }

        public async Task<bool> DeteleAccount(int id)
        {
            
                var checkAccount = await _context.Users.SingleOrDefaultAsync(s => s.Id == id);
                if (checkAccount != null)
                {
                    checkAccount.Status = false;
                    await base.SaveChange();
                    return true;
                }else
                {
                    return false;
                }
        }

        public async Task<AccountListDTO> GetDetailsUser(int id)
        {
            var userDetail = await _context.Users.SingleOrDefaultAsync(s => s.Id == id);

            if (userDetail != null)
            {
                var accountListDTO = _mapper.Map<AccountListDTO>(userDetail);
                return accountListDTO;
            }else
            {
                return null;
            }
        }

        public async Task<List<AccountListDTO>> GetListAccount()
        {
            var listAccount = _context.Users.ToList();

            var accountListDTOs = _mapper.Map<List<AccountListDTO>>(listAccount);

            return accountListDTOs;
        }

        public async Task<List<AccountRoleDTO>> GetListRole()
        {
            var listRole = _context.Roles.ToList();

            var roleListDTO = _mapper.Map<List<AccountRoleDTO>>(listRole);

            return roleListDTO;

        }

        public async Task<bool> UpdateRoleOfAccount(int id, int newRoleId)
        {
            var token = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _inforFromToken.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new Exception("User to be updated does not exist.");
            }

            var userRole = user.UserRoles.SingleOrDefault(ur => ur.UserId == id);
            if (userRole == null)
            {
                throw new Exception("UserRole entry does not exist.");
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == newRoleId);
            if (!roleExists)
            {
                throw new Exception("New role does not exist.");
            }

            userRole.RoleId = newRoleId;

            user.UpdateBy = userId; 
            user.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }


    }
}
