﻿using Microsoft.EntityFrameworkCore;
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
                if(checkRequest.Note == "Đã xác nhận")
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
                if (startDate > endDate)
                {
                    throw new Exception("Start date must be earlier than or equal to end date.");
                }
                if (vehicleId.HasValue && vehicleId < 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }
                if (vehicleOwnerId.HasValue && vehicleOwnerId < 0)
                {
                    throw new Exception("Invalid vehicle owner ID.");
                }
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
                vehicleId = x.VehicleId,
                LicenseVehicle = _context.Vehicles.Where(v => v.Id == x.VehicleId).Select(x => x.LicensePlate).FirstOrDefault(),
                DriverId = x.DriverId,
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
        private async Task<TotalPayementRentDriver> GetRentDriverTotalForOwner(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwner)
        {
            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }


            IQueryable<PaymentRentDriver> query = _context.PaymentRentDrivers.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            if (vehicleId != 0 && vehicleId.HasValue)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            else
            {
                query = query.Include(x => x.HistoryRentDriver).ThenInclude(hrd => hrd.Vehicle).Where(x => x.HistoryRentDriver.Vehicle.VehicleOwner == vehicleOwner);
            }
            var result = await GetRentDriver(query);
            if (result == null)
            {
                throw new Exception("No rent driver data found for the specified criteria.");
            }

            return result;

        }
        private Task<TotalPayementRentDriver> GetRentDriverTotalForStaff(DateTime startDate, DateTime endDate, int? vehicleId, int? vehicleOwner)
        {
            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }

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
            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }


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

        public async Task<List<DriverHistoryDTO>> GetDriverHistoryByUserIdAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var historyList = await _context.HistoryRentDrivers
                    .Join(_context.Vehicles,
                          hrd => hrd.VehicleId,
                          v => v.Id,
                          (hrd, v) => new { hrd, v })
                    .Join(_context.Drivers,
                          joined => joined.hrd.DriverId,
                          d => d.Id,
                          (joined, d) => new
                          {
                              joined.hrd,
                              joined.v,
                              DriverName = d.Name,
                              LicensePlate = joined.v.LicensePlate
                          })
                    .Where(joined => joined.hrd.DriverId == userId)
                    .Select(joined => new DriverHistoryDTO
                    {
                        HistoryId = joined.hrd.HistoryId,
                        DriverName = joined.DriverName,
                        LicensePlate = joined.LicensePlate,
                        TimeStart = joined.hrd.TimeStart,
                        EndStart = joined.hrd.EndStart,
                        CreatedAt = joined.hrd.CreatedAt,
                        CreatedBy = joined.hrd.CreatedBy,
                        UpdatedAt = joined.hrd.UpdateAt,
                        UpdatedBy = joined.hrd.UpdateBy
                    })
                    .ToListAsync();

                return historyList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching driver history: {ex.Message}");
            }
        }


        public async Task<List<DriverRentInfoDTO>> GetDriverRentInfo(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.HistoryRentDrivers.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(hrd => hrd.TimeStart >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(hrd => hrd.EndStart <= endDate.Value);
                
                var rentInfo = await query
                    .Join(_context.PaymentRentDrivers,
                        hrd => hrd.HistoryId,
                        prd => prd.HistoryRentDriverId,
                        (hrd, prd) => new { hrd, prd })
                    .Join(_context.Drivers,
                        joined => joined.hrd.DriverId,
                        d => d.Id,
                        (joined, d) => new { joined.hrd, joined.prd, d })
                    .Join(_context.Vehicles,
                        joined => joined.hrd.VehicleId,
                        v => v.Id,
                        (joined, v) => new DriverRentInfoDTO
                        {
                            HistoryId = joined.hrd.HistoryId,
                            DriverName = joined.d.Name,
                            VehicleLicense = v.LicensePlate,
                            TimeStart = joined.hrd.TimeStart,
                            EndStart = joined.hrd.EndStart,
                            Price = joined.prd.Price
                        })
                    .ToListAsync();

                return rentInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetDriverRentInfo: {ex.Message}");
            }
        }


        public async Task<List<DriverHistoryDTO>> GetHistoryByVehicleOwnerAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int vehicleOwnerId = _tokenHelper.GetIdInHeader(token);

                if (vehicleOwnerId == -1)
                {
                    throw new Exception("Invalid vehicle owner ID from token.");
                }

                var historyList = await _context.HistoryRentDrivers
                    .Join(_context.Vehicles,
                          hrd => hrd.VehicleId,
                          v => v.Id,
                          (hrd, v) => new { hrd, v })
                    .Join(_context.Drivers,
                          joined => joined.hrd.DriverId,
                          d => d.Id,
                          (joined, d) => new { joined.hrd, joined.v, DriverName = d.Name })
                    .Where(joined => joined.v.VehicleOwner == vehicleOwnerId)
                    .Select(joined => new DriverHistoryDTO
                    {
                        HistoryId = joined.hrd.HistoryId,
                        DriverName = joined.DriverName,
                        LicensePlate = joined.v.LicensePlate,
                        TimeStart = joined.hrd.TimeStart,
                        EndStart = joined.hrd.EndStart,
                        CreatedAt = joined.hrd.CreatedAt,
                        CreatedBy = joined.hrd.CreatedBy,
                        UpdatedAt = joined.hrd.UpdateAt,
                        UpdatedBy = joined.hrd.UpdateBy
                    })
                    .ToListAsync();

                return historyList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching history by vehicle owner: {ex.Message}");
            }
        }

        public async Task<List<DriverHistoryDTO>> getHistoryRentDriver(int userId, string role)
        {
            try
            {
                List<DriverHistoryDTO> history = new List<DriverHistoryDTO> ();
                var query = await (from htd in _context.HistoryRentDrivers join d in _context.Drivers
                                     on htd.DriverId equals d.Id join v in _context.Vehicles
                                     on htd.VehicleId equals v.Id join u in _context.Users
                                     on v.VehicleOwner equals u.Id join p in _context.PaymentRentDrivers
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
                                     }
                                     ).ToListAsync();
                if(role == "Staff")
                {
                     history = query;
                }
                if(role == "VehicleOwner")
                {
                     history = query.Where(x => x.vehicleOwnerId == userId).ToList();
                }
                if(role == "Driver")
                {
                     history = query.Where(x => x.DriverId == userId).ToList();
                }
                return history;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
