using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PaymentRentVehicleRepository : GenericRepository<PaymentRentVehicle>, IPaymentRentVehicleRepository
    {
        public PaymentRentVehicleRepository(SEP490_G67Context context) : base(context)
        {
        }

        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDate(DateTime startDate, DateTime endDate, int? carOwnerId, int? vehicleId)
        {
            try
            {
                var query = _context.PaymentRentVehicles
                            .Where(prv => prv.CreatedAt >= startDate && prv.CreatedAt <= endDate);
                if (carOwnerId.HasValue && carOwnerId != 0)
                {
                    query = query.Where(prv => prv.CarOwnerId == carOwnerId);
                }
                if (vehicleId.HasValue && vehicleId != 0)
                {
                    query = query.Where(prv => prv.VehicleId == vehicleId);
                }
                var getListPaymentRentVehicle = await query.Select(x => new PaymentRentVehicelDTO
                {
                        CreatedAt = x.CreatedAt,
                        DriverId = x.DriverId,
                        Price = x.Price ?? 0,
                        VehicleId = x.VehicleId,
                        CarOwnerId = x.CarOwnerId,
                    
                }).ToListAsync();
               

                var sumPaymentRentVehicel = query.Sum(x => x.Price);
                var combine = new TotalPaymentRentVehicleDTO
                {
                    PaymentRentVehicelDTOs = getListPaymentRentVehicle,
                    Total = sumPaymentRentVehicel,
                };
                if (!combine.PaymentRentVehicelDTOs.Any())
                {
                    throw new Exception("No data found for the given filters.");
                }
                return combine;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
