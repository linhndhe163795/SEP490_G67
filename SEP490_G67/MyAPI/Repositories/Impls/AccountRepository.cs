using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleOwnerDTOs;
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

        public async Task<bool> SoftDeleteVehicleOwner(int id)
        {
            var vehicleOwner = await _context.Users.FindAsync(id);
            if (vehicleOwner == null)
            {
                return false;
            }

            vehicleOwner.Status = false;
            vehicleOwner.UpdateAt = DateTime.Now;

            _context.Users.Update(vehicleOwner);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateVehicleOwner(int id, UpdateVehicleOwnerDTO vehicleOwnerDTO, int staffId)
        {
            var vehicleOwner = await _context.Users.FindAsync(id);
            if (vehicleOwner == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(vehicleOwnerDTO.Username))
            {
                var existingUsername = await _context.Users
                    .AnyAsync(v => v.Username == vehicleOwnerDTO.Username && v.Id != id);
                if (existingUsername)
                {
                    throw new Exception("Username already exists. Please provide a different username.");
                }
                vehicleOwner.Username = vehicleOwnerDTO.Username;
            }

            if (!string.IsNullOrEmpty(vehicleOwnerDTO.Email))
            {
                var existingEmail = await _context.Users
                    .AnyAsync(v => v.Email == vehicleOwnerDTO.Email && v.Id != id);
                if (existingEmail)
                {
                    throw new Exception("Email already exists. Please provide a different email.");
                }
                vehicleOwner.Email = vehicleOwnerDTO.Email;
            }

            vehicleOwner.NumberPhone = vehicleOwnerDTO.NumberPhone ?? vehicleOwner.NumberPhone;
            vehicleOwner.FullName = vehicleOwnerDTO.FullName ?? vehicleOwner.FullName;
            vehicleOwner.Address = vehicleOwnerDTO.Address ?? vehicleOwner.Address;
            vehicleOwner.Avatar = vehicleOwnerDTO.Avatar ?? vehicleOwner.Avatar;
            vehicleOwner.Status = vehicleOwnerDTO.Status ?? vehicleOwner.Status;
            vehicleOwner.Dob = vehicleOwnerDTO.Dob ?? vehicleOwner.Dob;
            vehicleOwner.UpdateBy = staffId;
            vehicleOwner.UpdateAt = DateTime.Now;

            _context.Users.Update(vehicleOwner);
            await _context.SaveChangesAsync();

            return true;
        }




    }
}
