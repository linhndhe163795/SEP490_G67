using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.RoleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly GetInforFromToken _inforFromToken;


        public RoleRepository(SEP490_G67Context _context, IMapper mapper,
            IHttpContextAccessor httpContextAccessor, GetInforFromToken inforFromToken) : base(_context)
        {
            _contextAccessor = httpContextAccessor;
            _inforFromToken = inforFromToken;
            _mapper = mapper;
        }

        public async Task<bool> AddRole(RoleAddDTO roleAddDTO)
        {
            var checkRoleNameExits = await _context.Roles.FirstOrDefaultAsync(s => s.RoleName.Equals(roleAddDTO.RoleName));

            if (checkRoleNameExits == null)
            {
                var role = new Role
                {
                    RoleName = roleAddDTO.RoleName,
                    CreatedAt = DateTime.Now,
                    Status = true,
                    UpdateAt = DateTime.Now,
                    CreatedBy = getUserId(),
                    UpdateBy = getUserId(),
                };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return true;
            }else
            {
                throw new Exception("Role name is exitss!!");
            }

        }

        public async Task<bool> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(s => s.Id == id);

            if (role == null)
            {
                throw new Exception("Role has no exits!!!");
            }else
            {
                role.Status = false;
                await _context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<RoleListDTO>> GetListRole()
        {
            var listRoles = await _context.Roles.ToListAsync();

            var roleListDTOs = _mapper.Map<List<RoleListDTO>>(listRoles);

            return roleListDTOs;
        }

        public async Task<bool> UpdateRole(int id, RoleAddDTO roleAddDTO)
        {
            var checkRoleExits = await _context.Roles.FirstOrDefaultAsync(s => s.Id == id);

            if(checkRoleExits != null)
            {
                checkRoleExits.Status = roleAddDTO.Status;
                checkRoleExits.UpdateBy = getUserId();
                checkRoleExits.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }else
            {
                throw new Exception("Role not found!!!");
            }

        }

        private int getUserId()
        {
            var token = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _inforFromToken.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            return userId;
        }
    }
}
