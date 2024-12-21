using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.DTOs.HistoryRentVehicles;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class HistoryRentVehicleRepository : GenericRepository<HistoryRentVehicle>, IHistoryRentVehicleRepository
    {
        private readonly SendMail _sendMail;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly IRequestRepository _requestRepository;
        private readonly IRequestDetailRepository _requestDetailRepository;
        private readonly IMapper _mapper;
        private readonly GetInforFromToken _getInforFromToken;
        public HistoryRentVehicleRepository(SEP490_G67Context context,
            SendMail sendMail, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper, IRequestRepository requestRepository, IRequestDetailRepository requestDetailRepository, IMapper mapper, GetInforFromToken getInforFromToken) : base(context)

        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _sendMail = sendMail;
            _requestRepository = requestRepository;
            _requestDetailRepository = requestDetailRepository;
            _mapper = mapper;
            _getInforFromToken = getInforFromToken;
        }
        //dành cho driver thuê xe 
        public async Task<bool> AccpetOrDeninedRentVehicle(AddHistoryVehicleUseRent add)
        {
            int requestId = add.requestId;
            bool choose = add.choose;
            int? vehicleId = add.vehicleId;
            decimal? price = add.price;

            var checkRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
            if (checkRequest == null)
            {
                throw new Exception("Request not found.");
            }
            if (checkRequest.TypeId != 7)
            {
                throw new Exception("Purpose of request is not rent vehicle");
            }
            if (checkRequest.Note == "Đã xác nhận")
            {
                throw new Exception("Request has been accepted before!");
            }
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);
            if (add.requestId < 0)
            {
                throw new Exception("Invalid request ID.");
            }
            if (!add.vehicleId.HasValue)
            {
                throw new Exception("Vehicle ID cannot be null.");
            }
            if (add.vehicleId.HasValue && add.vehicleId < 0)
            {
                throw new Exception("Invalid vehicle ID.");
            }

            if (add.price <= 0 && add.price != null)
            {
                throw new Exception("Price must be greater than 0.");
            }

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            try
            {
                var requestDetail = await (from r in _context.Requests
                                           join rd in _context.RequestDetails
                                           on r.Id equals rd.RequestId
                                           where r.Id == requestId
                                           select rd).FirstOrDefaultAsync();
                if (requestDetail == null)
                {
                    throw new Exception("Fail requestDetail!! in AccpetOrDeninedRentVehicle");
                }
                if (choose == false)
                {
                    var updateRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                    if (updateRequest == null)
                    {
                        throw new Exception("Request not found in AccpetOrDeninedRentVehicle");
                    }
                    updateRequest.Note = "Từ chối yêu cầu";
                    updateRequest.Status = true;
                    _context.Requests.Update(updateRequest);
                    await _context.SaveChangesAsync();
                    return true;
                }
                if (await checkVehicleNotDriver(vehicleId) && choose == true)
                {
                    var updateRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                    if (updateRequest == null)
                    {
                        throw new Exception("Request not found in AccpetOrDeninedRentVehicle");
                    }
                    updateRequest.Note = "Đã xác nhận";
                    updateRequest.Status = choose;

                    var updateRequestRentVehicle = await _requestRepository.UpdateRequestVehicleAsync(requestId, updateRequest);

                    requestDetail.VehicleId = vehicleId;
                    requestDetail.Price = price;
                    _context.RequestDetails.Update(requestDetail);
                    await _context.SaveChangesAsync();

                    var vechileAssgin = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                    vechileAssgin.DriverId = requestDetail.DriverId;
                    vechileAssgin.DateStartBusy = requestDetail.StartTime;
                    vechileAssgin.DateEndBusy = requestDetail.EndTime;
                    _context.Vehicles.Update(vechileAssgin);
                    await _context.SaveChangesAsync();

                    var vehicleOwner = await _context.Vehicles
                                                     .Where(s => s.Id == vehicleId)
                                                     .Select(vh => vh.VehicleOwner)
                                                     .FirstOrDefaultAsync();



                    var addHistoryVehicle = new HistoryRentVehicle
                    {
                        DriverId = requestDetail.CreatedBy,
                        VehicleId = requestDetail.VehicleId,
                        OwnerId = vehicleOwner,
                        TimeStart = requestDetail.StartTime,
                        EndStart = requestDetail.EndTime,
                        CreatedBy = requestDetail.CreatedBy,
                        CreatedAt = requestDetail.CreatedAt,
                        UpdateAt = DateTime.Now,
                        UpdateBy = requestDetail.CreatedBy,
                    };

                    await _context.HistoryRentVehicles.AddAsync(addHistoryVehicle);
                    await _context.SaveChangesAsync();
                    var addHispaymentVehicle = new PaymentRentVehicle
                    {
                        DriverId = requestDetail.CreatedBy,
                        VehicleId = requestDetail.VehicleId,
                        CarOwnerId = vehicleOwner,
                        Price = price,
                        HistoryRentVehicleId = addHistoryVehicle.HistoryId,
                        CreatedBy = requestDetail.CreatedBy,
                        CreatedAt = requestDetail.CreatedAt,
                        UpdateAt = DateTime.Now,
                        UpdateBy = requestDetail.CreatedBy,
                    };

                    await _context.PaymentRentVehicles.AddAsync(addHispaymentVehicle);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    throw new Exception("Not found vehicleAvaliable");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in AccpetOrDeninedRentVehicle: {ex.Message}");
            }
        }
        private async Task<bool> checkVehicleNotDriver(int? vehicleId)
        {
            try
            {
                var listVehicleNotDriver = await _context.Vehicles.Where(x => x.DriverId == null).ToListAsync();

                foreach (var vehicle in listVehicleNotDriver)
                {
                    if (vehicle.Id == vehicleId)
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
        public async Task<bool> createVehicleForUser(HistoryVehicleRentDTO historyVehicleDTO)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                var vehicleRent = new HistoryRentVehicle
                {
                    DriverId = historyVehicleDTO.DriverId,
                    VehicleId = historyVehicleDTO.VehicleId,
                    OwnerId = historyVehicleDTO.OwnerId,
                    TimeStart = historyVehicleDTO.TimeStart,
                    EndStart = historyVehicleDTO.EndStart,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    UpdateAt = DateTime.Now,
                    UpdateBy = userId,
                };
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create {ex.Message}");
            }
        }
        public async Task<List<VehicleConvenienceRentResponseDTO>> historyRentVehicleListDTOs(DateTime dateTime)
        {
            try
            {
                var ticket = await _context.Tickets.Where(t => t.TimeFrom.HasValue && t.TimeFrom.Value.Date <= dateTime.Date &&
                                        t.TimeFrom.Value.Date >= dateTime && t.TypeOfTicket == Constant.VE_BAO_XE).ToListAsync();
                var ticketVehicleIds = ticket
                                        .Where(t => t.VehicleId.HasValue)
                                        .Select(t => t.VehicleId.Value)
                                        .ToHashSet();

                var vehicles = await _context.Vehicles
                    .Include(x => x.Tickets)
                    .Where(v => v.VehicleTypeId == Constant.VE_XE_TIEN_CHUYEN
                           && !ticketVehicleIds.Contains(v.Id))
                    .ToListAsync();
                var vehicle = await (from v in _context.Vehicles
                                     join d in _context.Drivers
                                     on v.DriverId equals d.Id into driverGroup
                                     from d in driverGroup.DefaultIfEmpty()
                                     join u in _context.Users
                                     on v.VehicleOwner equals u.Id into userGroup
                                     from u in userGroup.DefaultIfEmpty()
                                     join t in _context.Tickets
                                     on v.Id equals t.VehicleId into ticketGroup
                                     from t in ticketGroup.DefaultIfEmpty()
                                     where v.VehicleTypeId == Constant.VE_XE_TIEN_CHUYEN && !ticketVehicleIds.Contains(v.Id)
                                     select new VehicleConvenienceRentResponseDTO
                                     {
                                         Id = v.Id,
                                         NumberSeat = v.NumberSeat,
                                         VehicleTypeId = v.VehicleTypeId,
                                         Status = v.Status,
                                         DriverId = d.Id != 0 ? d.Id : 0,
                                         VehicleOwner = u.Id,
                                         VehicleOwnerName = $"{u.Id} - Tên chủ xe: {u.FullName} - SĐT: {u.NumberPhone}",
                                         LicensePlate = $"{v.LicensePlate} - Số ghế: {v.NumberSeat}",
                                         DriverName = $"{(d.Name != null ? d.Name : null)} - SĐT: {d.NumberPhone}",
                                     }).Distinct().ToListAsync();
                

                return vehicle;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in HistoryRentVehicleListDTOs: {ex.Message}");
            }
        }



        public async Task<List<Vehicle>> GetAvailableVehicles(int requestId)
        {
            try
            {
                var request = await _context.RequestDetails
                    .Where(r => r.RequestId == requestId)
                    .Select(r => new
                    {
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        Seats = r.Seats
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    throw new Exception($"Request with ID {requestId} not found.");
                }

                var availableVehicles = await _context.Vehicles
                    .Where(v =>
                         (
                        v.DateStartBusy == null || v.DateEndBusy == null ||
                        (v.DateEndBusy <= request.StartTime || v.DateStartBusy >= request.EndTime)
                        ) &&
                        v.NumberSeat >= request.Seats &&
                        v.Status == true &&
                        v.Flag == false &&
                        v.DriverId == null
                    )
                    .ToListAsync();

                var result = availableVehicles.Select(v => new Vehicle
                {
                    Id = v.Id,
                    NumberSeat = v.NumberSeat,
                    VehicleTypeId = v.VehicleTypeId,
                    Status = v.Status,
                    Image = v.Image,
                    DriverId = v.DriverId,
                    VehicleOwner = v.VehicleOwner,
                    Description = v.Description,
                    LicensePlate = v.LicensePlate
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetAvailableVehicles: {ex.Message}");
            }
        }



        public bool IsUserRole(User user, string roleName)
        {
            if (user?.UserRoles == null) return false;
            return user.UserRoles.Any(ur => string.Equals(ur.Role?.RoleName, roleName));
        }
        public async Task<bool> sendMailRequestRentVehicle(string description)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                string[] greetings = { "Xin chào", "Chào bạn", "Thân gửi" };
                string greeting = greetings[new Random().Next(greetings.Length)];
                string bodyContent = $"{greeting} {user.FullName},\n\n" +
                                     description;

                SendMailDTO mail = new SendMailDTO
                {
                    FromEmail = "nhaxenhanam@gmail.com",
                    Password = "vzgq unyk xtpt xyjp",
                    ToEmail = user.Email,
                    Subject = "Yêu cầu thuê xe con - Liên hệ ngay",
                    Body = bodyContent
                };

                var checkMail = await _sendMail.SendEmail(mail);
                if (!checkMail)
                {
                    Console.WriteLine($"Failed to send email to {user.Email}");
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(1 + new Random().Next(10)));
            }

            return true;
        }
        public async Task<List<HistoryVehicleRentDTO>> listHistoryRentVehicle(int userId, string roleName)
        {
            try
            {
                List<HistoryVehicleRentDTO> history;

                var query = await (from hrv in _context.HistoryRentVehicles
                                   join d in _context.Drivers
                                       on hrv.DriverId equals d.Id
                                   join v in _context.Vehicles
                                       on hrv.VehicleId equals v.Id
                                   join u in _context.Users
                                       on v.VehicleOwner equals u.Id
                                   join p in _context.PaymentRentVehicles
                                       on hrv.HistoryId equals p.HistoryRentVehicleId
                                   select new HistoryVehicleRentDTO
                                   {
                                       Id = hrv.HistoryId,
                                       DriverId = hrv.DriverId,
                                       DriverName = d.Name,
                                       OwnerId = hrv.OwnerId,
                                       VehicleOwner = u.FullName,
                                       VehicleId = hrv.VehicleId,
                                       VehiclePrice = p.Price,
                                       TimeStart = hrv.TimeStart,
                                       EndStart = hrv.EndStart,
                                   }).ToListAsync();

                if (roleName == "Staff")
                {
                    history = query
                        .OrderByDescending(x => x.Id)
                        .ToList();
                }
                else if (roleName == "VehicleOwner")
                {
                    history = query
                        .Where(x => x.OwnerId == userId)
                        .OrderByDescending(x => x.Id)
                        .ToList();
                }
                else if (roleName == "Driver")
                {
                    history = query
                        .Where(x => x.DriverId == userId)
                        .OrderByDescending(x => x.Id)
                        .ToList();
                }
                else
                {
                    throw new Exception("Role not recognized");
                }

                return history;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }


    }
}
