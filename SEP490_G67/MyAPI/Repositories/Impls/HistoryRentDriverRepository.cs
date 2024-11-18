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

        public async Task<bool> AcceptOrDenyRentDriver(int requestId, bool choose)
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

                var requestDetail = await _context.Requests.Include(s => s.RequestDetails)
                                                           .SelectMany(s => s.RequestDetails)
                                                           .Where(s => s.RequestId == requestId)
                                                           .Select(rd => new
                                                           {
                                                               rd.CreatedBy,
                                                               rd.VehicleId,
                                                               rd.DriverId,
                                                               rd.StartTime,
                                                               rd.EndTime,
                                                               rd.CreatedAt,
                                                               rd.Price
                                                           }).FirstOrDefaultAsync();

                if (requestDetail == null)
                {
                    throw new Exception("Fail requestDetail in AcceptOrDenyRentDriver.");
                }

                var driver = await _context.Drivers
                                           .Where(d => d.Id == requestDetail.DriverId)
                                           .FirstOrDefaultAsync();

                if (driver == null)
                {
                    throw new Exception("Fail driver in AcceptOrDenyRentDriver.");
                }

                var updateRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                if (updateRequest == null)
                {
                    throw new Exception("Request not found in AcceptOrDenyRentDriver.");
                }

                updateRequest.Note = choose ? "Đã xác nhận" : "Từ chối xác nhận";
                updateRequest.Status = choose;

                var updateRequestRentDriver = await _requestRepository.UpdateRequestVehicleAsync(requestId, updateRequest);

                var updateRequestDetail = new RequestDetailDTO
                {
                    UpdatedBy = userId,
                    UpdatedAt = DateTime.Now,
                };

                var updateRequestDetailRentDriver = await _requestDetailRepository.CreateRequestDetailAsync(updateRequestDetail);

                if (!choose)
                {
                    return true;
                }

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

                var addPaymentDriver = new PaymentRentDriver
                {
                    DriverId = requestDetail.DriverId,
                    Price = requestDetail.Price,
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
            catch (Exception ex)
            {
                throw new Exception($"Error in AcceptOrDenyRentDriver: {ex.Message}");
            }
        }

        public async Task<object> GetRentDetailsWithTotalForOwner(DateTime startDate, DateTime endDate)
        {
            try
            {
                
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);
                
                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var vehicles = await _context.Vehicles
                    .Where(v => v.VehicleOwner == userId)
                    .Select(v => v.Id)
                    .ToListAsync();

                if (!vehicles.Any())
                {
                    throw new Exception("No vehicles found for this owner.");
                }

                var rentDetails = await (from hr in _context.HistoryRentDrivers
                                         join pr in _context.PaymentRentDrivers on hr.HistoryId equals pr.HistoryRentDriverId
                                         where vehicles.Contains((int)hr.VehicleId)
                                               && pr.CreatedAt >= startDate
                               && pr.CreatedAt <= endDate

                                         select new
                                         {
                                             hr.DriverId,
                                             pr.CreatedAt,
                                             pr.Price
                                         })
                                         .ToListAsync();

                decimal totalCost = (decimal)rentDetails.Sum(r => r.Price);

                return new
                {
                    RentDetails = rentDetails,
                    TotalCost = totalCost
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rent details for owner: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<HistoryRentDriverListDTOs>> GetListHistoryRentDriver()
        {
            try
            {
                int limit = 5;

                var result = new List<HistoryRentDriverListDTOs>();
                int takeCount = 0;
                int currentMinRentCount = 0;

                while (takeCount < limit)
                {
                    var driversWithCurrentRentCount = await _context.Drivers
                        .Select(d => new
                        {
                            Driver = d,
                            RentCount = _context.HistoryRentDrivers.Count(hrd => hrd.DriverId == d.Id)
                        })
                        .Where(d => d.RentCount > 0)
                        .Take(limit - takeCount)
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
                        .ToListAsync();

                    result.AddRange(driversWithCurrentRentCount);
                    takeCount += driversWithCurrentRentCount.Count;

                    if (takeCount >= limit)
                    {
                        break;
                    }

                    currentMinRentCount++;
                }

                return result.Take(limit);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetListHistoryRentDriver: {ex.Message}");
            }
        }
    }
}
