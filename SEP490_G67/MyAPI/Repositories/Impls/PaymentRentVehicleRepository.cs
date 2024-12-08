using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PaymentRentVehicleRepository : GenericRepository<PaymentRentVehicle>, IPaymentRentVehicleRepository
    {
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PaymentRentVehicleRepository(SEP490_G67Context context, GetInforFromToken getInforFromToken, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _getInforFromToken = getInforFromToken;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDate( int userId)
        {
            try
            {
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var role = _getInforFromToken.GetRoleFromToken(token);
                if (getInforUser == null)
                {
                    throw new Exception("User not found.");
                }
                if (role == "VehicleOwner")
                {
                    return await getPaymentRentVehicleByDateForVehicelOwner(getInforUser.Id);
                }
                if (role == "Staff")
                {
                    return await getPaymentRentVehicleByDateForStaff();
                }
                if (role == "Driver")
                {
                    return await getPaymentRentVehicleByDriver(getInforUser.Id);
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
        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDateForVehicelOwner(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("Invalid user ID.");
            }
            var query = _context.PaymentRentVehicles
                           .Where(prv => prv.CarOwnerId == userId);
            return await GetRevenueRentVehicle(query);
        }
        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDateForStaff()
        {
            try
            {
                var query = _context.PaymentRentVehicles.AsQueryable();

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
        public async Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDriver(int driverId)
        {
            if (driverId <= 0)
            {
                throw new Exception("Invalid user ID.");
            }
            var query = _context.PaymentRentVehicles
                           .Where(prv => prv.DriverId == driverId);
            return await GetRevenueRentVehicle(query);
        }
    }
}
