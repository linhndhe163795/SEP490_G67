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
                    throw new ArgumentNullException(nameof(lossCostAddDTOs), "LossCostAddDTOs cannot be null.");
                }
                if (lossCostAddDTOs.VehicleId <= 0)
                {
                    throw new ArgumentException("VehicleId must be a positive number.", nameof(lossCostAddDTOs.VehicleId));
                }

                if (lossCostAddDTOs.LossCostTypeId <= 0)
                {
                    throw new ArgumentException("LossCostTypeId must be a positive number.", nameof(lossCostAddDTOs.LossCostTypeId));
                }

                if (lossCostAddDTOs.Price <= 0)
                {
                    throw new ArgumentException("Price must be greater than zero.", nameof(lossCostAddDTOs.Price));
                }

                if (lossCostAddDTOs.DateIncurred == default || lossCostAddDTOs.DateIncurred > DateTime.Now)
                {
                    throw new ArgumentException("DateIncurred must be a valid date and not in the future.", nameof(lossCostAddDTOs.DateIncurred));
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
                var lossCostVehicle = await _context.LossCosts.Include(x => x.Vehicle).Include(x => x.LossCostType).OrderByDescending(ls => ls.Id)
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                Id = ls.Id,
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                VehicleOwnerId = ls.Vehicle.VehicleOwner,
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

        public async Task<TotalLossCost> GetLossCostVehicleByDate(int userId)
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
                    return await GetLossCosstForVehicleOwner(userId);
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetLossCosstForStaff(userId);
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
        private async Task<TotalLossCost> GetLossCosstForVehicleOwner(int? vehicleOwner)
        {
            var query =  _context.LossCosts.Include(x => x.Vehicle)
                                           .Include(x => x.LossCostType)
                                           .Where(x => x.Vehicle.VehicleOwner == vehicleOwner);
            var totalLossCost = query.Sum(x => x.Price);
            var lossCostVehicleByDate = await query
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                Id = ls.Id,
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostTypeId = ls.LossCostType.Id,
                                                LossCostType = ls.LossCostType.Description,
                                                VehicleOwner = _context.Users.Include(uv => uv.Vehicles).Where(u => u.Id == ls.Vehicle.VehicleOwner).Select(u => u.FullName).FirstOrDefault(),
                                                VehicleOwnerId = ls.Vehicle.VehicleOwner
                                            }).ToListAsync();
            var combineResult = new TotalLossCost
            {
                listLossCostVehicle = lossCostVehicleByDate,
                TotalCost = totalLossCost
            };
            return combineResult;
        }
        private async Task<TotalLossCost> GetLossCosstForStaff(int userId)
        {         
            var query =  _context.LossCosts.Include(x => x.Vehicle)
                                          .ThenInclude(x => x.VehicleOwnerNavigation)
                                          .Include(x => x.LossCostType).AsQueryable();
           
            var totalLossCost = query.Sum(x => x.Price);
            var lossCostVehicleByDate = await query
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                Id = ls.Id,
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostTypeId = ls.LossCostType.Id,
                                                LossCostType = ls.LossCostType.Description,
                                                VehicleOwner = _context.Users.Include(uv => uv.Vehicles).Where(u => u.Id == ls.Vehicle.VehicleOwner).Select(u => u.FullName).FirstOrDefault(),
                                                VehicleOwnerId = ls.Vehicle.VehicleOwner
                                            }).ToListAsync();
            var combineResult = new TotalLossCost
            {
                listLossCostVehicle = lossCostVehicleByDate,
                TotalCost = totalLossCost
            };
            return combineResult;
        }
        //update version
        public async Task<TotalLossCost> GetLossCostVehicleByDateUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId, int userId)
        {
            try
            {
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (getInforUser == null)
                {
                    throw new Exception("User not found.");
                }
                
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetLossCosstForStaffUpdate(startDate, endDate, vehicleId, userId);
                }
                throw new Exception("User role is not supported.");
            }
            catch (Exception ex)
            {
                throw new Exception("GetLossCostVehicleByDate: " + ex.Message);
            }
        }
        private async Task<TotalLossCost> GetLossCosstForStaffUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId, int userId)
        {

            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }

            if (vehicleId.HasValue && vehicleId <= 0)
            {
                throw new Exception("Invalid vehicle ID.");
            }
            if (!startDate.HasValue || !endDate.HasValue)
            {
                var now = DateTime.Now;
                startDate ??= new DateTime(now.Year, now.Month, 1); 
                endDate ??= new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)); 
            }
            var query = _context.LossCosts.Include(x => x.Vehicle)
                                          .ThenInclude(x => x.VehicleOwnerNavigation)
                                          .Include(x => x.LossCostType)
                                          .Where(x => x.DateIncurred >= startDate && x.DateIncurred <= endDate);
            
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }

            var totalLossCost = query.Sum(x => x.Price);
            var lossCostVehicleByDate = await query
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                Id = ls.Id,
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostType = ls.LossCostType.Description,
                                                VehicleOwner = _context.Users.Include(uv => uv.Vehicles).Where(u => u.Id == ls.Vehicle.VehicleOwner).Select(u => u.FullName).FirstOrDefault(),
                                                VehicleOwnerId = ls.Vehicle.VehicleOwner
                                            }).ToListAsync();

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
                if (id <= 0)
                {
                    throw new Exception("Invalid ID provided.");
                }
                if (id == null)
                {
                    throw new Exception("Invalid ID provided.");
                }
                if (lossCostupdateDTOs == null)
                {
                    throw new ArgumentNullException(nameof(lossCostupdateDTOs), "LossCostAddDTOs cannot be null.");
                }
                if (lossCostupdateDTOs.VehicleId <= 0)
                {
                    throw new ArgumentException("VehicleId must be a positive number.", nameof(lossCostupdateDTOs.VehicleId));
                }

                if (lossCostupdateDTOs.LossCostTypeId <= 0)
                {
                    throw new ArgumentException("LossCostTypeId must be a positive number.", nameof(lossCostupdateDTOs.LossCostTypeId));
                }

                if (lossCostupdateDTOs.Price <= 0)
                {
                    throw new ArgumentException("Price must be greater than zero.", nameof(lossCostupdateDTOs.Price));
                }

                if (lossCostupdateDTOs.DateIncurred == default || lossCostupdateDTOs.DateIncurred > DateTime.Now)
                {
                    throw new ArgumentException("DateIncurred must be a valid date and not in the future.", nameof(lossCostupdateDTOs.DateIncurred));
                }
                if (userId <= 0)
                {
                    throw new Exception("Invalid user ID provided.");
                }
                ValidateLossCostUpdateDTO(lossCostupdateDTOs);
                var lossCostId = await _context.LossCosts.FirstOrDefaultAsync(x => x.Id == id);
                if (lossCostId == null)
                {
                    throw new NullReferenceException(nameof(id));
                }
                if (lossCostupdateDTOs.DateIncurred.HasValue)
                {
                    lossCostId.DateIncurred = lossCostupdateDTOs.DateIncurred.Value;
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
                throw new Exception(ex.Message);
            }
        }
        private void ValidateLossCostUpdateDTO(LossCostUpdateDTO dto)
        {
            if (!dto.VehicleId.HasValue || dto.VehicleId <= 0)
            {
                throw new Exception("VehicleId is required and must be greater than 0.");
            }

            if (!dto.LossCostTypeId.HasValue || dto.LossCostTypeId <= 0)
            {
                throw new Exception("LossCostTypeId is required and must be greater than 0.");
            }

            if (!dto.Price.HasValue || dto.Price <= 0)
            {
                throw new Exception("Price is required and must be greater than 0.");
            }

            if (!dto.DateIncurred.HasValue)
            {
                throw new Exception("DateIncurred is required.");
            }
        }

    }
}
