﻿using AutoMapper;
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
        private readonly IMapper _mapper;

        public HistoryRentVehicleRepository(SEP490_G67Context context,
            SendMail sendMail, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper, IRequestRepository requestRepository, IRequestDetailRepository requestDetailRepository, IMapper mapper) : base(context)

        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _sendMail = sendMail;
            _requestRepository = requestRepository;
            _requestDetailRepository = requestDetailRepository;
            _mapper = mapper;
        }
        //dành cho driver thuê xe 
        public async Task<bool> AccpetOrDeninedRentVehicle(AddHistoryVehicleUseRent add)
        {
            int requestId = add.requestId;
            bool choose = add.choose;
            int? vehicleId = add.vehicleId;
            decimal price = add.price;

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

            if (add.price <= 0)
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
        //public string GetUserRole(int userId)
        //{
        //    // Kiểm tra nếu user là Driver
        //    var isDriver = _context.Drivers.Any(d => d.Id == userId);
        //    if (isDriver) return "Driver";

        //    // Kiểm tra vai trò trong UserRoles
        //    var user = _context.Users
        //        .Include(u => u.UserRoles)
        //        .ThenInclude(ur => ur.Role)
        //        .FirstOrDefault(u => u.Id == userId);

        //    if (user == null) throw new Exception("User not found");

        //    if (user.UserRoles.Any(ur => ur.Role.RoleName == "Staff")) return "Staff";
        //    if (user.UserRoles.Any(ur => ur.Role.RoleName == "VehicleOwner")) return "VehicleOwner";

        //    return "Unknown";
        //}
        //public async Task<List<HistoryVehicleRentDTO>> listHistoryRentVehicle(int userId)
        //{
        //    try
        //    {
        //        var role = GetUserRole(userId); // Lấy vai trò của User

        //        List<HistoryVehicleRentDTO> history;
        //        if (role == "Staff")
        //        {
        //            var listHistory = await _context.HistoryRentVehicles.ToListAsync();
        //            history = _mapper.Map<List<HistoryVehicleRentDTO>>(listHistory);
        //        }
        //        else if (role == "VehicleOwner")
        //        {
        //            var listHistory = await _context.HistoryRentVehicles
        //                .Where(h => h.OwnerId == userId)
        //                .ToListAsync();
        //            history = _mapper.Map<List<HistoryVehicleRentDTO>>(listHistory);
        //        }
        //        else if (role == "Driver")
        //        {
        //            var listHistory = await _context.HistoryRentVehicles
        //                .Where(h => h.DriverId == userId)
        //                .ToListAsync();
        //            history = _mapper.Map<List<HistoryVehicleRentDTO>>(listHistory);
        //        }
        //        else
        //        {
        //            throw new Exception("Role not recognized");
        //        }

        //        return history;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error: {ex.Message}");
        //    }
        //}

    }
}
