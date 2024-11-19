using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PaymentRentVehicleRepository : GenericRepository<PaymentRentVehicle>, IPaymentRentVehicleRepository
    {
        public PaymentRentVehicleRepository(SEP490_G67Context context) : base(context)
        {
        }

        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDate(DateTime startDate, DateTime endDate, int? carOwnerId, int? vehicleId, int userId)
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
                    return await getPaymentRentVehicleByDateForVehicelOwner(startDate, endDate, userId, vehicleId);
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await getPaymentRentVehicleByDateForStaff(startDate, endDate, carOwnerId, vehicleId);
                }
                throw new Exception("User role is not supported.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool IsUserRole(User user, string roleName)
        {
            return user.UserRoles.Any(ur => ur.Role.RoleName == roleName);
        }
        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDateForVehicelOwner(DateTime startDate, DateTime endDate, int userId, int? vehicleId)
        {
            var query = _context.PaymentRentVehicles
                           .Where(prv => prv.CreatedAt >= startDate && prv.CreatedAt <= endDate && prv.CarOwnerId == userId);
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            return await GetRevenueRentVehicle(query);
        }
        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDateForStaff(DateTime startDate, DateTime endDate, int? vehicleOwner, int? vehicleId)
        {
            var query = _context.PaymentRentVehicles
                           .Where(prv => prv.CreatedAt >= startDate && prv.CreatedAt <= endDate);
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            if(vehicleOwner.HasValue && vehicleOwner != 0)
            {
                query = query.Where(x => x.CarOwnerId == vehicleOwner);
            }
            return await GetRevenueRentVehicle(query);
        }
        private async Task<TotalPaymentRentVehicleDTO> GetRevenueRentVehicle(IQueryable<PaymentRentVehicle> query)
        {
            var listRentVehicle =
                 await query.Select(x => new PaymentRentVehicelDTO
                 {
                     CreatedAt = x.CreatedAt,
                     DriverId = x.DriverId,
                     Price = x.Price ?? 0,
                     VehicleId = x.VehicleId,
                     CarOwnerId = x.CarOwnerId,

                 }).ToListAsync();
            var sumPrice = query.Sum(x => x.Price);
            var combineResult = new TotalPaymentRentVehicleDTO
            {
                Total = sumPrice,
                PaymentRentVehicelDTOs = listRentVehicle
            };
            return combineResult;
        }
    }
}
