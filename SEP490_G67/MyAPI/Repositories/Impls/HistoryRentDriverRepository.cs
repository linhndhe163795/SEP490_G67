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

        public async Task<bool> AcceptOrDenyRentDriver(int requestId, bool choose, int? driverId)
        {
            try
            {
                var checkRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);

               
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
                    
                    var vechileAssgin = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == requestDetail.VehicleId);
                    vechileAssgin.DriverId = driverId;
                    _context.Vehicles.Update(vechileAssgin);
                    await _context.SaveChangesAsync();
                    
                    requestDetail.DriverId = driverId;
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
                        Price = requestDetail.Price,
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
                    if(driver.Id == driverId)
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

        public async Task<TotalPayementRentDriver> GetRentDetailsWithTotalForOwner(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwnerId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (IsUserRole(getInforUser, "VehicleOwner"))
                {
                    return await GetRentDriverTotalForOwner(startDate, endDate, vehicleId, userId);
                }
                else if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetRentDriverTotalForStaff(startDate, endDate, vehicleId, vehicleOwnerId);
                }
                else
                {
                    return await GetRentDriverTotalForDriver(startDate, endDate, userId);
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
            var rentDetails = await query.Select(x => new PaymentRentDriverDTO
            {
                Price = x.Price,
                LicenseVehicle = _context.Vehicles.Where(v => v.Id == x.VehicleId).Select(x => x.LicensePlate).FirstOrDefault(),
                DriverName = _context.Drivers.Where(d => d.Id == x.DriverId).Select(x => x.Name).FirstOrDefault(),
                CreatedAt = x.CreatedAt,
            }).ToListAsync();
            var total = query.Sum(x => x.Price);
            var combine = new TotalPayementRentDriver
            {
                Total = total,
                PaymentRentDriverDTOs = rentDetails
            };
            return combine;
        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForOwner(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwner)
        {
            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            if (vehicleId != 0 && vehicleId.HasValue)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            else
            {
                query = query.Include(x => x.HistoryRentDriver).ThenInclude(hrd => hrd.Vehicle).Where(x => x.HistoryRentDriver.Vehicle.VehicleOwner == vehicleOwner);
            }
            return GetRentDriver(query);

        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForStaff(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwner)
        {
            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            if (vehicleId != 0 && vehicleId.HasValue)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            else if (vehicleOwner != 0 && vehicleOwner.HasValue)
            {
                query = query.Include(x => x.HistoryRentDriver).ThenInclude(hrd => hrd.Vehicle).Where(x => x.HistoryRentDriver.Vehicle.VehicleOwner == vehicleOwner);
            }
            return GetRentDriver(query);
        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForDriver(DateTime startDate, DateTime endDate, int driverId)
        {
            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            if(driverId != 0)
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

        public async Task<bool> UpdateDriverInRequestAsync(int driverId, int requestId)
        {
            try
            {
                
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


    }
}
