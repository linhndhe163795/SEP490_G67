using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicleDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class LossCostVehicleRepository : GenericRepository<LossCost>, ILossCostVehicleRepository
    {
        private readonly IMapper _mapper;
        public LossCostVehicleRepository(SEP490_G67Context _context, IMapper mapper) : base(_context)
        {
            _mapper = mapper;
        }

        public async Task AddLossCost(LossCostAddDTOs lossCostAddDTOs, int userID)
        {
            try
            {
                if (lossCostAddDTOs == null)
                {
                    throw new NullReferenceException();
                }
                lossCostAddDTOs.CreatedBy = userID;
                lossCostAddDTOs.CreatedAt = DateTime.Now;
                var lossCostAddMapper = _mapper.Map<LossCost>(lossCostAddDTOs);
                _context.LossCosts.Add(lossCostAddMapper);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("AddLossCost: " + ex.Message);
            }
        }

        public async Task DeleteLossCost(int id)
        {
            try
            {
                var lossCostbyID = await _context.LossCosts.FirstOrDefaultAsync(x => x.Id == id);
                if (lossCostbyID == null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    _context.LossCosts.Remove(lossCostbyID);
                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception("DeleteLossCost: " + ex.Message);
            }
        }

        public async Task<List<AddLostCostVehicleDTOs>> GetAllLostCost()
        {
            try
            {
                var lossCostVehicle = await _context.LossCosts.Include(x => x.Vehicle).Include(x => x.LossCostType)
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                            }).ToListAsync();
                if (lossCostVehicle == null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    return lossCostVehicle;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("GetLossCostVehicleByDate: " + ex.Message);
            }
        }

        public async Task<TotalLossCost> GetLossCostVehicleByDate(int? vehicleId, DateTime startDate, DateTime endDate, int? vehicleOwnerId, int userId)
        {
            try
            {
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (getInforUser == null)
                {
                    throw new Exception("User not found.");
                }
                if (IsUserRole(getInforUser, "VehicleOwner"))
                {
                    return await GetLossCosstForVehicleOwner(startDate, endDate, userId, vehicleId);
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetLossCosstForStaff(startDate,endDate, vehicleOwnerId, vehicleId, userId);
                }
                throw new Exception("User role is not supported.");
            }
            catch (Exception ex)
            {
                throw new Exception("GetLossCostVehicleByDate: " + ex.Message);
            }
        }
        public bool IsUserRole(User user, string roleName)
        {
            return user.UserRoles.Any(ur => ur.Role.RoleName == roleName);
        }
        private async Task<TotalLossCost> GetLossCosstForVehicleOwner(DateTime? startDate, DateTime? endDate, int? vehicleOwner, int? vechileId)
        {
            var query =  _context.LossCosts.Include(x => x.Vehicle)
                                           .Include(x => x.LossCostType)
                                           .Where(x => x.DateIncurred >= startDate && 
                                                 x.DateIncurred <= endDate &&
                                                 x.Vehicle.VehicleOwner == vehicleOwner);
            if(vechileId.HasValue && vechileId != 0)
            {
                query = query.Where(x => x.VehicleId == vechileId);
            }

            var totalLossCost = query.Sum(x => x.Price);
            var lossCostVehicleByDate = await query
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostType = ls.LossCostType.Description,
                                                VehicleOwner = _context.Users.Include(uv => uv.Vehicles).Where(u => u.Id == ls.Vehicle.VehicleOwner).Select(u => u.FullName).FirstOrDefault()
                                            }).ToListAsync();
            //if (!lossCostVehicleByDate.Any())
            //{
            //    throw new Exception("No loss cost data found for the specified criteria.");
            //}
            var combineResult = new TotalLossCost
            {
                listLossCostVehicle = lossCostVehicleByDate,
                TotalCost = totalLossCost
            };
            return combineResult;
        }
        private async Task<TotalLossCost> GetLossCosstForStaff(DateTime startDate, DateTime endDate, int? vehicleOwner, int? vehicleId ,int userId)
        {
            var query = _context.LossCosts.Include(x => x.Vehicle)
                                          .ThenInclude(x => x.VehicleOwnerNavigation)
                                          .Include(x => x.LossCostType)
                                          .Where(x => x.DateIncurred >= startDate && x.DateIncurred <= endDate);
            if(vehicleOwner.HasValue && vehicleOwner != 0)
            {
                query = query.Where(x => x.Vehicle.VehicleOwner == vehicleOwner.Value);
            }
            if (vehicleId.HasValue && vehicleId != 0) 
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            
            var totalLossCost = query.Sum(x => x.Price);
            var lossCostVehicleByDate = await query
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostType = ls.LossCostType.Description,
                                                VehicleOwner = _context.Users.Include(uv => uv.Vehicles).Where(u => u.Id == ls.Vehicle.VehicleOwner).Select(u => u.FullName).FirstOrDefault()
                                            }).ToListAsync();
            //if (!lossCostVehicleByDate.Any())
            //{
            //    throw new Exception("No loss cost data found for the specified criteria.");
            //}
            var combineResult = new TotalLossCost
            {
                listLossCostVehicle = lossCostVehicleByDate,
                TotalCost = totalLossCost
            };
            return combineResult;
        }
        public async Task UpdateLossCostById(int id, LossCostUpdateDTO lossCostupdateDTOs, int userId)
        {
            try
            {
                var lossCostId = await _context.LossCosts.FirstOrDefaultAsync(x => x.Id == id);
                if (lossCostId == null)
                {
                    throw new NullReferenceException(nameof(id));
                }
                lossCostId.DateIncurred = lossCostupdateDTOs.DateIncurred;
                lossCostId.Price = lossCostupdateDTOs.Price;
                lossCostId.VehicleId = lossCostupdateDTOs.VehicleId;
                lossCostId.Description = lossCostupdateDTOs.Description;
                lossCostId.LossCostTypeId = lossCostupdateDTOs.LossCostTypeId;
                lossCostId.UpdateAt = DateTime.Now;
                lossCostId.UpdateBy = userId;
                _context.LossCosts.Update(lossCostId);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateLossCostById: " + ex.Message);
            }
        }
    }
}
