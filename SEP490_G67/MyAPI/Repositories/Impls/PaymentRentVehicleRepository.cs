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
                if (startDate > endDate)
                {
                    throw new Exception("Start date must be earlier than or equal to end date.");
                }

                if (vehicleId.HasValue && vehicleId <= 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }

                if (carOwnerId.HasValue && carOwnerId <= 0)
                {
                    throw new Exception("Invalid car owner ID.");
                }

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

            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }

            if (userId <= 0)
            {
                throw new Exception("Invalid user ID.");
            }

            if (vehicleId.HasValue && vehicleId <= 0)
            {
                throw new Exception("Invalid vehicle ID.");
            }

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
            try
            {
                if (startDate > endDate)
                {
                    throw new Exception("Start date must be earlier than or equal to end date.");
                }

                if (vehicleId.HasValue && vehicleId <= 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }

                if (vehicleOwner.HasValue && vehicleOwner <= 0)
                {
                    throw new Exception("Invalid vehicle owner ID.");
                }
                var query = _context.PaymentRentVehicles
                               .Where(prv => prv.CreatedAt >= startDate && prv.CreatedAt <= endDate);
                if (vehicleId.HasValue && vehicleId != 0)
                {
                    query = query.Where(x => x.VehicleId == vehicleId);
                }
                if (vehicleOwner.HasValue && vehicleOwner != 0)
                {
                    query = query.Where(x => x.CarOwnerId == vehicleOwner);
                }
                return await GetRevenueRentVehicle(query);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message, ex);
            }

        }
        private async Task<TotalPaymentRentVehicleDTO> GetRevenueRentVehicle(IQueryable<PaymentRentVehicle> query)
        {
            var listRentVehicle =
                 await query.Select(x => new PaymentRentVehicelDTO
                 {
                     Id = x.Id,
                     CreatedAt = x.CreatedAt,
                     driverId = x.DriverId,
                     DriverName = _context.Drivers.Where(d => d.Id == x.DriverId).Select(d => d.Name).FirstOrDefault(),
                     Price = x.Price ?? 0,
                     vehicelId = x.VehicleId,
                     LicenseVehicle = _context.Vehicles.Where(v => v.Id == x.DriverId).Select(v => v.LicensePlate).FirstOrDefault(),
                     CarOwner = _context.Users.Include(uv => uv.Vehicles).Where(u => u.Id == x.CarOwnerId).Select(u => u.FullName).FirstOrDefault()
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
