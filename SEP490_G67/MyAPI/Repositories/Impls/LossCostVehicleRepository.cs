using AutoMapper;
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
                                                LossCostTypeId = ls.LossCostTypeId,
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

        public async Task<TotalLossCost> GetLossCostVehicleByDate(int? vehicleId, DateTime? startDate, DateTime? endDate, int? vehicleOwnerId)
        {
            try
            {
                var query = _context.LossCosts
                                            .Include(x => x.Vehicle)
                                            .Include(x => x.LossCostType)
                                            .Where(x => x.DateIncurred >= startDate && x.DateIncurred <= endDate);
               
                if (vehicleId.HasValue && vehicleId != 0)
                {
                    query = query.Where(x => x.VehicleId == vehicleId);
                }
                else if (vehicleOwnerId.HasValue && vehicleOwnerId != 0)
                {
                    query = query.Where(x => x.Vehicle.VehicleOwner == vehicleOwnerId);
                }
                else
                {
                    throw new ArgumentException("Either vehicleId or vehicleOwnerId must be provided.");
                }
                var lossCostVehicleByDate = await query
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                VehicleId = ls.VehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostTypeId = ls.LossCostTypeId,
                                                VehicleOwner = ls.Vehicle.VehicleOwner
                                            }).ToListAsync();
              
                if (!lossCostVehicleByDate.Any())
                {
                    throw new Exception("No loss cost data found for the specified criteria.");
                }
                var totalCost = lossCostVehicleByDate.Sum(x => x.Price);

                var response = new TotalLossCost
                {
                    listLossCostVehicle = lossCostVehicleByDate,
                    TotalCost = totalCost
                };

                if (lossCostVehicleByDate == null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("GetLossCostVehicleByDate: " + ex.Message);
            }
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
