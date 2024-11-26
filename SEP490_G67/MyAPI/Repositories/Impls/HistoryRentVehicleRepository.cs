using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.DTOs.HistoryRentVehicles;
using MyAPI.DTOs.RequestDTOs;
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

        public HistoryRentVehicleRepository(SEP490_G67Context context, 
            SendMail sendMail, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper, IRequestRepository requestRepository, IRequestDetailRepository requestDetailRepository) : base(context)

        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _sendMail = sendMail;
            _requestRepository = requestRepository;
            _requestDetailRepository = requestDetailRepository; 
        }
        //dành cho driver thuê xe 
        public async Task<bool> AccpetOrDeninedRentVehicle(int requestId, bool choose, int? vehicleId)
        {
            var checkRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);


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
                if(choose == false)
                {
                    var updateRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                    if (updateRequest == null)
                    {
                        throw new Exception("Request not found in AccpetOrDeninedRentVehicle");
                    }
                    updateRequest.Note = "Từ chối yêu cầu";
                    updateRequest.Status = choose;
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
                    _context.RequestDetails.Update(requestDetail);
                    await _context.SaveChangesAsync();

                    var vechileAssgin = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                    vechileAssgin.DriverId = requestDetail.DriverId;
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
                        Price = requestDetail.Price,
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

        public async Task<List<HistoryRentVehicleListDTO>> historyRentVehicleListDTOs()
        {
            try
            {
                int limit = 5;

                var vehiclesNeverRented = await _context.Vehicles
                    .Where(v => v.DriverId == null)
                    .Select(v => new
                    {
                        Vehicle = v,
                        RentCount = _context.HistoryRentVehicles.Count(hrv => hrv.VehicleId == v.Id)
                    })
                    .Where(v => v.RentCount == 0)
                    .Take(limit)
                    .Select(v => new HistoryRentVehicleListDTO
                    {
                        Id = v.Vehicle.Id,
                        NumberSeat = v.Vehicle.NumberSeat,
                        VehicleTypeId = v.Vehicle.VehicleTypeId,
                        Status = v.Vehicle.Status,
                        Image = v.Vehicle.Image,
                        DriverId = v.Vehicle.DriverId,
                        VehicleOwner = v.Vehicle.VehicleOwner,
                        Description = v.Vehicle.Description,
                        LicensePlate = v.Vehicle.LicensePlate,
                    })
                    .ToListAsync();

                if (vehiclesNeverRented.Count < limit)
                {
                    int remainingLimit = limit - vehiclesNeverRented.Count;

                    var additionalVehicles = await _context.Vehicles
                        .Where(v => v.DriverId == null)
                        .Select(v => new
                        {
                            Vehicle = v,
                            RentCount = _context.HistoryRentVehicles.Count(hrv => hrv.VehicleId == v.Id)
                        })
                        .Where(v => v.RentCount > 0)
                        .OrderBy(v => v.RentCount)
                        .Take(remainingLimit)
                        .Select(v => new HistoryRentVehicleListDTO
                        {
                            Id = v.Vehicle.Id,
                            NumberSeat = v.Vehicle.NumberSeat,
                            VehicleTypeId = v.Vehicle.VehicleTypeId,
                            Status = v.Vehicle.Status,
                            Image = v.Vehicle.Image,
                            DriverId = v.Vehicle.DriverId,
                            VehicleOwner = v.Vehicle.VehicleOwner,
                            Description = v.Vehicle.Description,
                            LicensePlate = v.Vehicle.LicensePlate,
                        })
                        .ToListAsync();

                    vehiclesNeverRented.AddRange(additionalVehicles);
                }

                return vehiclesNeverRented.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in historyRentVehicleListDTOs: {ex.Message}");
            }
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

        

    }
}
