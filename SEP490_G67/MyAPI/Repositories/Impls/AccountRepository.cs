using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
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
                    checkAccount.Status = !checkAccount.Status;
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
                accountListDTO.Role = _context.UserRoles
                        .Where(ur => ur.UserId == userDetail.Id)
                        .Join(_context.Roles,
                              ur => ur.RoleId,
                              role => role.Id,
                              (ur, role) => role.RoleName)
                        .FirstOrDefault();
                return accountListDTO;
            }else
            {
                return null;
            }
        }

        public async Task<List<AccountListDTO>> GetListAccount()
        {
            var listAccount = await _context.Users
                .Select(user => new AccountListDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    NumberPhone = user.NumberPhone,
                    Password = user.Password,
                    Avatar = user.Avatar,
                    FullName = user.FullName,
                    Address = user.Address,
                    Status = user.Status,
                    Dob = user.Dob,
                    CreatedAt = user.CreatedAt,
                    CreatedBy = user.CreatedBy,
                    UpdateAt = user.UpdateAt,
                    UpdateBy = user.UpdateBy,
                    Role = _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                              ur => ur.RoleId,
                              role => role.Id,
                              (ur, role) => role.RoleName)
                        .FirstOrDefault() 
                })
                .ToListAsync();

            return listAccount;
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

            _context.UserRoles.Remove(userRole);

            var newUserRole = new UserRole
            {
                UserId = id,
                RoleId = newRoleId,
                Status = true  
            };
            _context.UserRoles.Add(newUserRole);

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
                throw new Exception("Vehicle owner not found.");
            }

            if (string.IsNullOrEmpty(vehicleOwnerDTO.Username))
            {
                throw new Exception("Username cannot be null or empty.");
            }
            var existingUsername = await _context.Users
                .AnyAsync(v => v.Username == vehicleOwnerDTO.Username && v.Id != id);
            if (existingUsername)
            {
                throw new Exception("Username already exists. Please choose a different one.");
            }
            vehicleOwner.Username = vehicleOwnerDTO.Username;

            if (string.IsNullOrEmpty(vehicleOwnerDTO.Email))
            {
                throw new Exception("Email cannot be null or empty.");
            }
            var existingEmail = await _context.Users
                .AnyAsync(v => v.Email == vehicleOwnerDTO.Email && v.Id != id);
            if (existingEmail)
            {
                throw new Exception("Email already exists. Please choose a different one.");
            }
            vehicleOwner.Email = vehicleOwnerDTO.Email;

            if (string.IsNullOrEmpty(vehicleOwnerDTO.NumberPhone))
            {
                throw new Exception("Phone number cannot be null or empty.");
            }
            vehicleOwner.NumberPhone = vehicleOwnerDTO.NumberPhone;

            if (string.IsNullOrEmpty(vehicleOwnerDTO.FullName))
            {
                throw new Exception("Full name cannot be null or empty.");
            }
            vehicleOwner.FullName = vehicleOwnerDTO.FullName;

            if (string.IsNullOrEmpty(vehicleOwnerDTO.Address))
            {
                throw new Exception("Address cannot be null or empty.");
            }
            vehicleOwner.Address = vehicleOwnerDTO.Address;

            if (!vehicleOwnerDTO.Dob.HasValue)
            {
                throw new Exception("Date of birth cannot be null.");
            }
            vehicleOwner.Dob = vehicleOwnerDTO.Dob;

            vehicleOwner.Avatar = vehicleOwnerDTO.Avatar ?? vehicleOwner.Avatar;
            vehicleOwner.Status = vehicleOwnerDTO.Status ?? vehicleOwner.Status;
            vehicleOwner.UpdateBy = staffId;
            vehicleOwner.UpdateAt = DateTime.Now;

            _context.Users.Update(vehicleOwner);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<VehicleOwnerDTO>> listVehicleOnwer()
        {
            try
            {
                var listVehicleOwner = await (from u in _context.Users
                                             join ur in _context.UserRoles on u.Id equals ur.UserId
                                             join r in _context.Roles on ur.RoleId equals r.Id
                                             where r.RoleName == "VehicleOwner"
                                             select new VehicleOwnerDTO
                                             {
                                                 Id = u.Id,
                                                 Username = u.Username,
                                                 Email = u.Email
                                             }).ToListAsync();
                if(listVehicleOwner == null)
                {
                    throw new Exception("Not found vehicle owner");
                }

                return listVehicleOwner;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while fetching vehicle owners: {ex.Message}");
            }
        }
    }
}
