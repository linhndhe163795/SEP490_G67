using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.Models;
using MyAPI.Infrastructure.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Helper;
using DocumentFormat.OpenXml.Wordprocessing;
using MyAPI.DTOs.PaymentRentDriver;
using Microsoft.EntityFrameworkCore.Internal;

namespace MyAPI.Repositories.Impls
{
    public class HistoryRentDriverRepository : GenericRepository<HistoryRentDriver>, IHistoryRentDriverRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly IRequestRepository _requestRepository;
        private readonly IRequestDetailRepository _requestDetailRepository;

        public HistoryRentDriverRepository(
            SEP490_G67Context context,
            IHttpContextAccessor httpContextAccessor,
            GetInforFromToken tokenHelper,
            IRequestRepository requestRepository,
            IRequestDetailRepository requestDetailRepository
        ) : base(context)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _requestRepository = requestRepository;
            _requestDetailRepository = requestDetailRepository;
        }
        public async Task<bool> AcceptOrDenyRentDriver(AddHistoryRentDriver add)
        {
            int requestId = add.requestId;
            bool choose = add.choose;
            int? driverId = add.driverId;
            decimal price = add.price;
            if (add.requestId <= 0)
            {
                throw new Exception("Invalid request ID.");
            }
            if (!add.driverId.HasValue)
            {
                throw new Exception("Driver ID cannot be null.");
            }
            if (add.driverId.HasValue && add.driverId <= 0)
            {
                throw new Exception("Invalid driver ID.");
            }
            if (add.price == null)
            {
                throw new Exception("Price cannot be null.");
            }
            if (add.price <= 0)
            {
                throw new Exception("Price must be greater than 0.");
            }
            try
            {
                var checkRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                if (checkRequest == null)
                {
                    throw new Exception("Request not found.");
                }

                if (checkRequest.TypeId != 4)
                {
                    throw new Exception("Purpose of request is not rent driver");
                }
                if (checkRequest.Note == "Đã xác nhận")
                {
                    throw new Exception("Request has been accepted!");
                }
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var requestDetail = await (from r in _context.Requests
                                           join rd in _context.RequestDetails
                                           on r.Id equals rd.RequestId
                                           where r.Id == requestId
                                           select rd).FirstOrDefaultAsync();

                if (requestDetail == null)
                {
                    throw new Exception("Fail requestDetail in AcceptOrDenyRentDriver.");
                }
                if (!choose)
                {
                    var updateRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                    if (updateRequest == null)
                    {
                        throw new Exception("Request not found in AcceptOrDenyRentDriver.");
                    }

                    updateRequest.Note = choose ? "Đã xác nhận" : "Từ chối xác nhận";
                    updateRequest.Status = choose;
                    var updateRequestRentDriver = await _requestRepository.UpdateRequestVehicleAsync(requestId, updateRequest);
                    return true;
                }


                if (await checkDriverNotVechile(driverId))
                {
                    var updateRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                    if (updateRequest == null)
                    {
                        throw new Exception("Request not found in AcceptOrDenyRentDriver.");
                    }

                    updateRequest.Note = choose ? "Đã xác nhận" : "Từ chối xác nhận";
                    updateRequest.Status = choose;
                    var updateRequestRentDriver = await _requestRepository.UpdateRequestVehicleAsync(requestId, updateRequest);

                    //var vechileAssgin = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == requestDetail.VehicleId);
                    //vechileAssgin.DriverId = driverId;
                    //_context.Vehicles.Update(vechileAssgin);
                    //await _context.SaveChangesAsync();

                    requestDetail.DriverId = driverId;
                    requestDetail.Price = price;
                    requestDetail.UpdateBy = userId;
                    requestDetail.UpdateAt = DateTime.Now;
                    _context.RequestDetails.Update(requestDetail);
                    await _context.SaveChangesAsync();

                    var addHistoryDriver = new HistoryRentDriver
                    {
                        DriverId = requestDetail.DriverId,
                        VehicleId = requestDetail.VehicleId,
                        TimeStart = requestDetail.StartTime,
                        EndStart = requestDetail.EndTime,
                        CreatedBy = requestDetail.CreatedBy,
                        CreatedAt = requestDetail.CreatedAt,
                        UpdateAt = DateTime.Now,
                        UpdateBy = requestDetail.CreatedBy,
                    };
                    await _context.HistoryRentDrivers.AddAsync(addHistoryDriver);
                    await _context.SaveChangesAsync();
                    var addPaymentDriver = new PaymentRentDriver
                    {
                        DriverId = requestDetail.DriverId,
                        Price = price,
                        VehicleId = requestDetail.VehicleId,
                        Description = _context.Requests.SingleOrDefault(x => x.Id == requestId).Description,
                        HistoryRentDriverId = addHistoryDriver.HistoryId,
                        CreatedBy = requestDetail.CreatedBy,
                        CreatedAt = requestDetail.CreatedAt,
                        UpdateAt = DateTime.Now,
                        UpdateBy = requestDetail.CreatedBy,
                    };

                    await _context.PaymentRentDrivers.AddAsync(addPaymentDriver);
                    await _context.SaveChangesAsync();

                    return true;
                }
                else
                {
                    throw new Exception("Not found driver avaliable");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in AcceptOrDenyRentDriver: {ex.Message}");
            }
        }
        private async Task<bool> checkDriverNotVechile(int? driverId)
        {
            try
            {
                var listDriver = await (from d in _context.Drivers
                                        join v in _context.Vehicles on d.Id equals v.DriverId into vehicleGroup
                                        from v in vehicleGroup.DefaultIfEmpty()
                                        where v == null
                                        select d).ToListAsync();
                foreach (var driver in listDriver)
                {
                    if (driver.Id == driverId)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwner()
        {
            try
            {

                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);
                var role = _tokenHelper.GetRoleFromToken(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (role == "VehicleOwner")
                {
                    return await GetRentDriverTotalForOwner(userId);
                }
                else if (role == "Staff")
                {
                    return await GetRentDriverTotalForStaff();
                }
                else if (role == "Driver")
                {
                    return await GetRentDriverTotalForDriver(userId);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rent details for owner: {ex.Message}");
                throw;
            }
        }
        public async Task<TotalPayementRentDriver> GetRentDriver(IQueryable<PaymentRentDriver> query)
        {
            var rentDetails = await query
                .Join(_context.Vehicles, rent => rent.VehicleId, vehicle => vehicle.Id, (rent, vehicle) => new { rent, vehicle })
                .Join(_context.Users, rv => rv.vehicle.VehicleOwner, user => user.Id, (rv, user) => new { rv.rent, rv.vehicle, VehicleOwnerName = user.FullName })
                .Join(_context.Drivers, rv => rv.rent.DriverId, driver => driver.Id, (rv, driver) => new
                {
                    rv.rent.Id,
                    rv.rent.Price,
                    rv.rent.VehicleId,
                    LicenseVehicle = rv.vehicle.LicensePlate,
                    rv.rent.DriverId,
                    DriverName = driver.Name,
                    rv.rent.CreatedAt,
                    VehicleOwnerName = rv.VehicleOwnerName
                })
                .Select(x => new PaymentRentDriverDTO
                {
                    Id = x.Id,
                    Price = x.Price,
                    vehicleOwner = x.VehicleOwnerName,
                    vehicleId = x.VehicleId,
                    LicenseVehicle = x.LicenseVehicle,
                    DriverId = x.DriverId,
                    DriverName = x.DriverName,
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync();
            var total = query.Sum(x => x.Price);
            var combine = new TotalPayementRentDriver
            {
                Total = total,
                PaymentRentDriverDTOs = rentDetails
            };

            return combine;
        }
        private async Task<TotalPayementRentDriver> GetRentDriverTotalForOwner(int? vehicleOwner)
        {



            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.AsQueryable();
            query = query.Include(x => x.HistoryRentDriver).ThenInclude(hrd => hrd.Vehicle).Where(x => x.HistoryRentDriver.Vehicle.VehicleOwner == vehicleOwner);

            var result = await GetRentDriver(query);
            if (result == null)
            {
                throw new Exception("No rent driver data found for the specified criteria.");
            }

            return result;

        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForStaff()
        {


            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.AsQueryable();

            return GetRentDriver(query);
        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForDriver(int driverId)
        {
            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.AsQueryable();
            if (driverId != 0)
            {
                query = query.Where(x => x.DriverId == driverId);
            }
            return GetRentDriver(query);
        }
        private bool IsUserRole(User user, string roleName)
        {
            return user.UserRoles.Any(ur => ur.Role.RoleName == roleName);
        }
        public async Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver()
        {
            try
            {
                int limit = 5;

                var driversWithRentCount = await _context.Drivers
                    .Where(d => !_context.Vehicles.Any(v => v.DriverId == d.Id))
                    .Select(d => new
                    {
                        Driver = d,
                        RentCount = _context.HistoryRentDrivers.Count(hrd => hrd.DriverId == d.Id)
                    })
                    .OrderBy(d => d.RentCount)
                    .ThenBy(d => d.Driver.Id)
                    .ToListAsync();


                var result = driversWithRentCount
                    .Select(d => new HistoryRentDriverListDTOs
                    {
                        Id = d.Driver.Id,
                        UserName = d.Driver.UserName,
                        Name = d.Driver.Name,
                        NumberPhone = d.Driver.NumberPhone,
                        License = d.Driver.License,
                        Avatar = d.Driver.Avatar,
                        Dob = d.Driver.Dob,
                        StatusWork = d.Driver.StatusWork,
                        TypeOfDriver = d.Driver.TypeOfDriver,
                        Status = d.Driver.Status,
                        Email = d.Driver.Email
                    })
                    .Take(limit)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetListHistoryRentDriver: {ex.Message}");
            }
        }

        public async Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriverUpdate(int requestDeatailsId)
        {
            try
            {
                var requestDetails = await _context.RequestDetails.FirstOrDefaultAsync(x => x.RequestId == requestDeatailsId);
                var listDriverBusyIds = await (from hrd in _context.HistoryRentDrivers
                                               where hrd.TimeStart < requestDetails.EndTime
                                                     && hrd.EndStart > requestDetails.StartTime
                                               select hrd.DriverId).Distinct().ToListAsync();

                var availableDrivers = await _context.Drivers
                                        .Include(d => d.Vehicles).Where(d => !_context.Vehicles.Any(v => v.DriverId == d.Id) &&
                                            !listDriverBusyIds.Contains(d.Id))
                                            .Select(d => new HistoryRentDriverListDTOs
                                            {
                                                Id = d.Id,
                                                UserName = d.UserName,
                                                Name = d.Name,
                                                NumberPhone = d.NumberPhone,
                                            }).ToListAsync();

                return availableDrivers;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetListHistoryRentDriver: {ex.Message}");
            }
        }

        public async Task<bool> UpdateDriverInRequestAsync(int driverId, int requestId)
        {
            try
            {
                if (driverId < 0)
                {
                    throw new Exception("Invalid driver ID.");
                }

                if (requestId < 0)
                {
                    throw new Exception("Invalid request ID.");
                }
                var requestDetail = await _context.RequestDetails.FirstOrDefaultAsync(rd => rd.RequestId == requestId);

                if (requestDetail == null)
                {
                    return false;
                }


                requestDetail.DriverId = driverId;


                _context.RequestDetails.Update(requestDetail);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<DriverHistoryDTO>> getHistoryRentDriver(int userId, string role)
        {
            try
            {
                List<DriverHistoryDTO> history = new List<DriverHistoryDTO>();
                var query = await (from htd in _context.HistoryRentDrivers
                                   join d in _context.Drivers
                                     on htd.DriverId equals d.Id
                                   join v in _context.Vehicles
                                     on htd.VehicleId equals v.Id
                                   join u in _context.Users
                                     on v.VehicleOwner equals u.Id
                                   join p in _context.PaymentRentDrivers
                                     on htd.HistoryId equals p.HistoryRentDriverId
                                   select new DriverHistoryDTO
                                   {
                                       HistoryId = htd.HistoryId,
                                       DriverId = htd.DriverId,
                                       DriverName = d.Name ?? "",
                                       vehicleOwnerId = u.Id,
                                       vehicleOwner = u.FullName ?? "",
                                       LicensePlate = v.LicensePlate ?? "",
                                       price = p.Price,
                                       TimeStart = htd.TimeStart,
                                       EndStart = htd.EndStart
                                   }).ToListAsync();

                if (role == "Staff")
                {
                    history = query.OrderByDescending(x => x.HistoryId).ToList();
                }
                else if (role == "VehicleOwner")
                {
                    history = query
                        .Where(x => x.vehicleOwnerId == userId)
                        .OrderByDescending(x => x.HistoryId)
                        .ToList();
                }
                else if (role == "Driver")
                {
                    history = query
                        .Where(x => x.DriverId == userId)
                        .OrderByDescending(x => x.HistoryId)
                        .ToList();
                }

                return history;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // update vesion 2 
        public async Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwnerUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            try
            {
                if (startDate > endDate)
                {
                    throw new Exception("Start date must be earlier than or equal to end date.");
                }
                if (vehicleId.HasValue && vehicleId < 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);
                var role = _tokenHelper.GetRoleFromToken(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();

                if (role == "Staff" || role == "VehicleOwner")
                {
                    return await GetRentDriverTotalForStaffUpdate(startDate, endDate, vehicleId);
                }
                else if (role == "Driver")
                {
                    return await GetRentDriverTotalForDriverUpdate(startDate, endDate, userId, vehicleId);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rent details for owner: {ex.Message}");
                throw;
            }
        }
        public async Task<TotalPayementRentDriver> GetRentDriverUpdate(IQueryable<PaymentRentDriver> query)
        {
            var rentDetails = await query
                .Join(_context.Vehicles, rent => rent.VehicleId, vehicle => vehicle.Id, (rent, vehicle) => new { rent, vehicle })
                .Join(_context.Users, rv => rv.vehicle.VehicleOwner, user => user.Id, (rv, user) => new { rv.rent, rv.vehicle, VehicleOwnerName = user.FullName })
                .Join(_context.Drivers, rv => rv.rent.DriverId, driver => driver.Id, (rv, driver) => new
                {
                    rv.rent.Id,
                    rv.rent.Price,
                    rv.rent.VehicleId,
                    LicenseVehicle = rv.vehicle.LicensePlate,
                    rv.rent.DriverId,
                    DriverName = driver.Name,
                    rv.rent.CreatedAt,
                    VehicleOwnerName = rv.VehicleOwnerName
                })
                .Select(x => new PaymentRentDriverDTO
                {
                    Id = x.Id,
                    Price = x.Price,
                    vehicleOwner = x.VehicleOwnerName,
                    vehicleId = x.VehicleId,
                    LicenseVehicle = x.LicenseVehicle,
                    DriverId = x.DriverId,
                    DriverName = x.DriverName,
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync();
            var total = query.Sum(x => x.Price);
            var combine = new TotalPayementRentDriver
            {
                Total = total,
                PaymentRentDriverDTOs = rentDetails
            };

            return combine;
        }
        private async Task<TotalPayementRentDriver> GetRentDriverTotalForOwnerUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }
            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.Include(x => x.HistoryRentDriver).ThenInclude(hrd => hrd.Vehicle);
            if (startDate.HasValue && endDate.HasValue && !vehicleId.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            }
            if (startDate.HasValue && !endDate.HasValue && !vehicleId.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate );
            }
            if (!startDate.HasValue && endDate.HasValue && !vehicleId.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= endDate);
            }
            if (startDate.HasValue && endDate.HasValue && vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate && x.VehicleId == vehicleId);
            }
            if (vehicleId != 0 && vehicleId.HasValue)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            var result = await GetRentDriverUpdate(query);
            if (result == null)
            {
                throw new Exception("No rent driver data found for the specified criteria.");
            }
            return result;
        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForStaffUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }

            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers;
            if (startDate.HasValue && endDate.HasValue && vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate && x.VehicleId == vehicleId);
            }
            if (vehicleId != 0 && vehicleId.HasValue)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }

            return GetRentDriverUpdate(query);
        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForDriverUpdate(DateTime? startDate, DateTime? endDate, int driverId, int? vehicleId)
        {
            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }


            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            if (driverId != 0)
            {
                query = query.Where(x => x.DriverId == driverId);
            }
            if (vehicleId != null)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            return GetRentDriverUpdate(query);
        }
        // end update




    }
}
