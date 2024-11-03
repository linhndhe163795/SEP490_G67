using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class LossCostVehicleRepository : GenericRepository<LossCost>, ILossCostVehicleRepository
    {
        public LossCostVehicleRepository(SEP490_G67Context _context) : base(_context)
        {

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

        public async Task<TotalLossCost> GetLossCostVehicleByDate(int? vehicleId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
       
               
                var lossCostVehicleByDate = await _context.LossCosts.Include(x => x.Vehicle).Include(x => x.LossCostType)
                                            .Where(x => x.VehicleId == vehicleId && x.DateIncurred >= startDate && x.DateIncurred <= endDate)
                                            .Select(ls => new AddLostCostVehicleDTOs
                                            {
                                                VehicleId = vehicleId,
                                                LicensePlate = ls.Vehicle.LicensePlate,
                                                DateIncurred = ls.DateIncurred,
                                                Description = ls.Description,
                                                Price = ls.Price,
                                                LossCostTypeId = ls.LossCostTypeId,
                                            }).ToListAsync();
                if (!lossCostVehicleByDate.Any())
                {
                    throw new Exception("No loss cost data found for the specified vehicle and date range.");
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
    }
}
