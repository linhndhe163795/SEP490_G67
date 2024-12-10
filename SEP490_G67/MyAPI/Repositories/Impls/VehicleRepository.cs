using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MyAPI.DTOs;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq;
using System.Text.RegularExpressions;


namespace MyAPI.Repositories.Impls
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly IRequestRepository _requestRepository;
        private readonly IRequestDetailRepository _requestDetailRepository;
        private readonly SendMail _sendMail;
        private readonly ITripRepository _tripRepository;




        public VehicleRepository(SEP490_G67Context context, IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            GetInforFromToken tokenHelper,
            IRequestRepository requestRepository,
            IRequestDetailRepository requestDetailRepository,
            ITripRepository tripRepository,
            SendMail sendMail) : base(context)

        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _requestRepository = requestRepository;
            _requestDetailRepository = requestDetailRepository;
            _sendMail = sendMail;
            _tripRepository = tripRepository;
        }

        public async Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO)
        {
            try
            {
                var licensePlatePattern = @"^\d{2}[A-Z]-\d{5}$";
                if (vehicleAddDTO.NumberSeat == null || vehicleAddDTO.NumberSeat <= 0)
                {
                    throw new Exception("NumberSeat must be greater than 0.");
                }
                if (string.IsNullOrWhiteSpace(vehicleAddDTO.LicensePlate))
                {
                    throw new Exception("License plate cannot be null or empty.");
                }
                if (!Regex.IsMatch(vehicleAddDTO.LicensePlate, licensePlatePattern))
                {
                    throw new Exception("Invalid license plate format. Expected format: 99A-99999.");
                }
                if (vehicleAddDTO.VehicleTypeId != Constant.XE_TIEN_CHUYEN && vehicleAddDTO.VehicleTypeId != Constant.XE_DU_LICH && vehicleAddDTO.VehicleTypeId != Constant.XE_LIEN_TINH)
                {
                    throw new Exception("Invalid type of vehicle");
                }
                var checkUserNameDrive = await _context.Drivers.SingleOrDefaultAsync(s => s.UserName.Equals(vehicleAddDTO.driverName));
                if (checkUserNameDrive == null && vehicleAddDTO.driverName != null)
                {
                    throw new Exception("Not found driver");
                }
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception("User does not exist.");
                }

                var checkLicensePlate = await _context.Vehicles.FirstOrDefaultAsync(s => s.LicensePlate.Equals(vehicleAddDTO.LicensePlate));
                if (checkLicensePlate != null)
                {
                    throw new Exception("License plate is duplicate.");
                }


                var isRoleStaff = await _context.UserRoles
                                .Include(x => x.Role)
                                .AnyAsync(u => u.UserId == userId && u.Role.RoleName == "Staff");

                Vehicle vehicle = new Vehicle
                {
                    NumberSeat = vehicleAddDTO.NumberSeat.Value,
                    VehicleTypeId = vehicleAddDTO.VehicleTypeId ?? 1,
                    Image = vehicleAddDTO.Image,
                    Status = isRoleStaff,
                    DriverId = vehicleAddDTO.driverId,
                    VehicleOwner = (isRoleStaff == true) ? vehicleAddDTO.VehicleOwner : userId,
                    LicensePlate = vehicleAddDTO.LicensePlate,
                    Description = vehicleAddDTO.Description,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = null,
                    UpdateBy = null,
                };

                _context.Vehicles.Add(vehicle);

                if (!isRoleStaff)
                {
                    var requestDTO = new RequestDTO
                    {
                        TypeId = 1,
                        Description = "Request to add vehicle",
                        Note = "Pending approval",
                    };

                    var createdRequest = await _requestRepository.CreateRequestVehicleAsync(requestDTO);
                    if (createdRequest == null)
                    {
                        throw new Exception("Failed to create request.");
                    }

                    var requestDetailDTO = new RequestDetailDTO
                    {
                        RequestId = createdRequest.Id,
                        VehicleId = vehicle.Id,
                        Seats = vehicle.NumberSeat,
                    };

                    await _requestDetailRepository.CreateRequestDetailAsync(requestDetailDTO);

                    SendMailDTO mail = new SendMailDTO
                    {
                        FromEmail = "nhaxenhanam@gmail.com",
                        Password = "vzgq unyk xtpt xyjp",
                        ToEmail = user.Email,
                        Subject = "Notification about vehicle registration in the system",
                        Body = "Your information has been received. Please wait for approval."
                    };

                    var checkMail = await _sendMail.SendEmail(mail);
                    if (!checkMail)
                    {
                        throw new Exception("Failed to send email.");
                    }
                }

                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception("AddVehicleAsync: " + ex.Message);
            }
        }

        public async Task<bool> AddVehicleByStaffcheckAsync(int requestId, bool isApprove)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                if (requestId == null)
                {
                    throw new ArgumentNullException(nameof(requestId), "Request ID cannot be null.");
                }

                if (requestId <= 0)
                {
                    throw new Exception("Request ID must be greater than 0.");
                }

                var checkRequestExits = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
                if (checkRequestExits == null)
                {
                    throw new Exception("Request does not exist.");
                }
                if (checkRequestExits.TypeId != 1)
                {
                    throw new Exception("Request not add vehicle");
                }


                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception("User does not exist.");
                }

                checkRequestExits.Note = isApprove ? "Đã xác nhận" : "Từ chối xác nhận";
                checkRequestExits.Status = true;
                var updateRequest = await _requestRepository.UpdateRequestVehicleAsync(requestId, checkRequestExits);

                if (!updateRequest)
                {
                    throw new Exception("Failed to update request.");
                }

                var emailOfUser = await _context.Users
                                                .Where(rq => rq.Id == checkRequestExits.UserId)
                                                .Select(rq => rq.Email)
                                                .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(emailOfUser))
                {
                    throw new Exception("User email not found.");
                }

                SendMailDTO mail = new SendMailDTO
                {
                    FromEmail = "nhaxenhanam@gmail.com",
                    Password = "vzgq unyk xtpt xyjp",
                    ToEmail = emailOfUser,
                    Subject = "Thông báo về việc đăng ký xe vào hệ thống",
                    Body = isApprove
                        ? "Hệ thống đã xác nhận yêu cầu xe của bạn. Xe của bạn đã tham gia hệ thống."
                        : "Rất tiếc, yêu cầu xe của bạn đã bị từ chối xác nhận."
                };

                var checkMail = await _sendMail.SendEmail(mail);
                if (!checkMail)
                {
                    throw new Exception("Send mail fail!!");
                }

                var vehicleID = await _context.Requests
                                              .Where(rq => rq.Id == checkRequestExits.Id)
                                              .SelectMany(rq => rq.RequestDetails)
                                              .Select(rq => rq.VehicleId)
                                              .FirstOrDefaultAsync();
                var requestDetails = await _context.RequestDetails.FirstOrDefaultAsync(x => x.RequestId == requestId);


                if (vehicleID == 0 || vehicleID == null)
                {
                    throw new Exception("Vehicle ID not found in request details.");
                }

                await UpdateVehicleByStaff(vehicleID.Value, user.Id, isApprove);
                await _context.SaveChangesAsync();
                return isApprove;
            }
            catch (Exception ex)
            {
                throw new Exception("AddVehicleByStaffcheckAsync: " + ex.Message);
            }
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            try
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id), "Vehicle ID cannot be null.");
                }

                if (id <= 0)
                {
                    throw new Exception("Vehicle ID must be greater than 0.");
                }

                var checkVehicle = await _context.Vehicles.SingleOrDefaultAsync(s => s.Id == id);

                if (checkVehicle == null)
                {
                    throw new Exception("Vehicle not found.");
                }

                checkVehicle.Status = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("DeleteVehicleAsync: " + ex.Message);
            }
        }

        public async Task<List<EndPointDTO>> GetListEndPointByVehicleId(int vehicleId)
        {
            try
            {
                if (vehicleId == null)
                {
                    throw new ArgumentNullException(nameof(vehicleId), "Vehicle ID cannot be null.");
                }

                if (vehicleId <= 0)
                {
                    throw new Exception("Vehicle ID must be greater than 0.");
                }

                var i = 1;
                var listStartPoint = await (from v in _context.Vehicles
                                            join vt in _context.VehicleTrips
                                            on v.Id equals vt.VehicleId
                                            join t in _context.Trips
                                            on vt.TripId equals t.Id
                                            where v.Id == vehicleId
                                            select t.PointEnd).Distinct()
                                         .ToListAsync();

                if (listStartPoint == null || !listStartPoint.Any())
                {
                    throw new Exception("No endpoints found for the given Vehicle ID.");
                }

                List<EndPointDTO> listEndPointDTOs = new List<EndPointDTO>();
                foreach (var v in listStartPoint)
                {
                    listEndPointDTOs.Add(new EndPointDTO { id = i++, name = v });
                }

                return listEndPointDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetListEndPointByVehicleId: " + ex.Message);
            }
        }

        public async Task<List<VehicleTypeDTO>> GetVehicleTypeDTOsAsync()
        {
            try
            {

                var listVehicleType = await _context.VehicleTypes.ToListAsync();

                var vehicleTypeListDTOs = _mapper.Map<List<VehicleTypeDTO>>(listVehicleType);

                return vehicleTypeListDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetVehicleTypeDTOsAsync: " + ex.Message);
            }

        }

        public async Task<bool> UpdateVehicleAsync(int id, string driverName)
        {
            try
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id), "Vehicle ID cannot be null.");
                }

                if (id <= 0)
                {
                    throw new Exception("Vehicle ID must be greater than 0.");
                }

                if (string.IsNullOrWhiteSpace(driverName))
                {
                    throw new Exception("Driver name cannot be null or whitespace.");
                }

                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var vehicleUpdate = await _context.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);
                if (vehicleUpdate == null)
                {
                    throw new Exception("Vehicle not found.");
                }

                var checkUserNameDrive = await _context.Drivers.SingleOrDefaultAsync(s => s.Name.Equals(driverName));
                if (checkUserNameDrive == null)
                {
                    throw new Exception("Driver name not found in the system.");
                }

                vehicleUpdate.DriverId = userId;
                vehicleUpdate.UpdateBy = checkUserNameDrive.Id;
                vehicleUpdate.UpdateAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateVehicleAsync: " + ex.Message);
            }
        }

        public async Task<List<StartPointDTO>> GetListStartPointByVehicleId(int vehicleId)
        {
            try
            {
                if (vehicleId == null)
                {
                    throw new ArgumentNullException(nameof(vehicleId), "Vehicle ID cannot be null.");
                }

                if (vehicleId <= 0)
                {
                    throw new Exception("Vehicle ID must be greater than 0.");
                }

                var listStartPoint = await (from v in _context.Vehicles
                                            join vt in _context.VehicleTrips
                                            on v.Id equals vt.VehicleId
                                            join t in _context.Trips
                                            on vt.TripId equals t.Id
                                            where v.Id == vehicleId
                                            select t.PointStart).Distinct().ToListAsync();

                if (listStartPoint == null || !listStartPoint.Any())
                {
                    throw new Exception("No start points found for the given Vehicle ID.");
                }

                var listStartPointDTOs = listStartPoint.Select((v, i) => new StartPointDTO
                {
                    id = i + 1,
                    name = v
                }).ToList();

                return listStartPointDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetListStartPointByVehicleId: " + ex.Message);
            }
        }

        public bool IsUserRole(User user, string roleName)
        {
            return user.UserRoles.Any(ur => ur.Role.RoleName == roleName);
        }
        public async Task<List<VehicleListDTO>> GetVehicleDTOsAsync(int userId, string role)
        {
            try
            {
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (getInforUser == null)
                {
                    throw new Exception("Invalid user");
                }

                List<Vehicle> listVehicle = new List<Vehicle>();
                if (role == "Staff")
                {
                    listVehicle = await _context.Vehicles.ToListAsync();
                }
                if (role == "VehicleOwner")
                {
                    listVehicle = await _context.Vehicles.Where(x => x.VehicleOwner == userId).ToListAsync();
                }
                if (role == "Driver")
                {
                    listVehicle = await _context.Vehicles.Where(x => x.DriverId == userId).ToListAsync();
                }

                var vehicleListDTOs = _mapper.Map<List<VehicleListDTO>>(listVehicle);

                return vehicleListDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetVehicleDTOsAsync: " + ex.Message);
            }
        }

        //public async Task<bool> UpdateVehicleAsync(int id, string driverName, int userIdUpdate)
        //{
        //    try
        //    {
        //        if (id == null)
        //        {
        //            throw new ArgumentNullException(nameof(id), "Vehicle ID cannot be null.");
        //        }

        //        if (id <= 0)
        //        {
        //            throw new Exception("Vehicle ID must be greater than 0.");
        //        }

        //        if (string.IsNullOrWhiteSpace(driverName))
        //        {
        //            throw new Exception("Driver name cannot be null or whitespace.");
        //        }

        //        if (userIdUpdate == null)
        //        {
        //            throw new ArgumentNullException(nameof(userIdUpdate), "User ID for update cannot be null.");
        //        }

        //        if (userIdUpdate <= 0)
        //        {
        //            throw new Exception("User ID for update must be greater than 0.");
        //        }

        //        var vehicleUpdate = await _context.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);
        //        if (vehicleUpdate == null)
        //        {
        //            throw new Exception("Vehicle not found.");
        //        }

        //        var checkUserNameDrive = await _context.Drivers.SingleOrDefaultAsync(s => s.Name.Equals(driverName));
        //        if (checkUserNameDrive == null)
        //        {
        //            throw new Exception("Driver name not found in the system.");
        //        }

        //        vehicleUpdate.DriverId = userIdUpdate;
        //        vehicleUpdate.UpdateBy = checkUserNameDrive.Id;
        //        vehicleUpdate.UpdateAt = DateTime.Now;

        //        await _context.SaveChangesAsync();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("UpdateVehicleAsync: " + ex.Message);
        //    }
        //}

        private async Task UpdateVehicleByStaff(int? id, int? userIdUpdate, bool updateStatus)
        {
            try
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id), "Vehicle ID cannot be null.");
                }

                if (id <= 0)
                {
                    throw new Exception("Vehicle ID must be greater than 0.");
                }

                if (userIdUpdate == null)
                {
                    throw new ArgumentNullException(nameof(userIdUpdate), "User ID for update cannot be null.");
                }

                if (userIdUpdate <= 0)
                {
                    throw new Exception("User ID for update must be greater than 0.");
                }

                var vehicleUpdate = _context.Vehicles.SingleOrDefault(vehicle => vehicle.Id == id);

                if (vehicleUpdate == null)
                {
                    throw new Exception("Vehicle not found in the system.");
                }

                vehicleUpdate.Status = updateStatus;
                vehicleUpdate.UpdateBy = userIdUpdate.Value;
                vehicleUpdate.UpdateAt = DateTime.Now;

                _context.Vehicles.Update(vehicleUpdate);
                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                throw new Exception("UpdateVehicleByStaff: " + ex.Message);
            }
        }
        public async Task<bool> AssignDriverToVehicleAsync(int vehicleId, int driverId)
        {
            try
            {
                if (vehicleId == null)
                {
                    throw new ArgumentNullException(nameof(vehicleId), "Vehicle ID cannot be null.");
                }

                if (vehicleId <= 0)
                {
                    throw new Exception("Vehicle ID must be greater than 0.");
                }

                if (driverId == null)
                {
                    throw new ArgumentNullException(nameof(driverId), "Driver ID cannot be null.");
                }

                if (driverId <= 0)
                {
                    throw new Exception("Driver ID must be greater than 0.");
                }

                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                {
                    throw new Exception("Vehicle not found.");
                }
                var driver = await _context.Drivers.FindAsync(driverId);

                if (driver == null)
                {
                    throw new Exception("Not Found Driver");
                }
                vehicle.DriverId = driverId;
                _context.Vehicles.Update(vehicle);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public async Task<int> GetNumberSeatAvaiable(int tripId, DateTime dateTime)
        {
            if (tripId == null)
            {
                throw new ArgumentNullException(nameof(tripId), "Trip ID cannot be null.");
            }

            if (tripId <= 0)
            {
                throw new Exception("Trip ID must be greater than 0.");
            }

            if (dateTime == null)
            {
                throw new ArgumentNullException(nameof(dateTime), "DateTime cannot be null.");
            }

            try
            {
                var ticketCount = await _tripRepository.GetTicketCount(tripId, dateTime);

                var vehicleTrip = await _context.VehicleTrips.FirstOrDefaultAsync(x => x.TripId == tripId);
                if (vehicleTrip == null)
                {
                    throw new Exception("VehicleTrip not found for the given Trip ID.");
                }

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleTrip.VehicleId);
                if (vehicle == null || vehicle.NumberSeat == null)
                {
                    throw new Exception("Vehicle not found or NumberSeat is null.");
                }

                var seatAvailable = vehicle.NumberSeat.Value - ticketCount;
                return seatAvailable;
            }
            catch (Exception ex)
            {
                throw new Exception("GetNumberSeatAvaiable: " + ex.Message);
            }
        }
        public async Task<VehicleAddDTO> GetVehicleById(int vehicleId)
        {
            if (vehicleId == null)
            {
                throw new ArgumentNullException(nameof(vehicleId), "Vehicle ID cannot be null.");
            }

            if (vehicleId <= 0)
            {
                throw new Exception("Vehicle ID must be greater than 0.");
            }

            try
            {
                //var vehicleById = await _context.Vehicles.Include(u => u.Users).FirstOrDefaultAsync(x => x.Id == vehicleId);
                var vehicle = await (from v in _context.Vehicles
                                     join u in _context.Users
                                     on v.VehicleOwner equals u.Id
                                     join vt in _context.VehicleTypes on v.VehicleTypeId equals vt.Id
                                     where v.Id == vehicleId
                                     select new VehicleAddDTO
                                     {
                                         Image = v.Image,
                                         driverName = v.Driver.Name ?? null,
                                         LicensePlate = v.LicensePlate,
                                         NumberSeat = v.NumberSeat,
                                         VehicleOwner = v.VehicleOwner,
                                         Description = v.Description,
                                         vehicleOwnerName = u.FullName,
                                         driverId = v.DriverId ?? null,
                                         VehicleTypeId = vt.Id,
                                         vehicleTypeName = vt.Description,
                                         Status = v.Status,
                                     }).FirstOrDefaultAsync();
                if (vehicle == null)
                {
                    throw new Exception("Vehicle not found.");
                }

                //var vehicleMapper = _mapper.Map<VehicleAddDTO>(vehicleById);
                return vehicle;
            }
            catch (Exception ex)
            {
                throw new Exception("GetVehicleById: " + ex.Message);
            }
        }
        public Task<List<VehicleLicenscePlateDTOs>> getLicensecePlate()
        {
            try
            {
                var list = _context.Vehicles.Select(x => new VehicleLicenscePlateDTOs
                {
                    Id = x.Id,
                    LicensePlate = x.LicensePlate
                }).ToListAsync();
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<int> VehicleByDriverId(int driverId)
        {
            if (driverId == null)
            {
                throw new ArgumentNullException(nameof(driverId), "Driver ID cannot be null.");
            }

            if (driverId <= 0)
            {
                throw new Exception("Driver ID must be greater than 0.");
            }

            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.DriverId == driverId);
                if (vehicle == null)
                {
                    throw new Exception("Driver does not have any vehicle.");
                }
                return vehicle.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("VehicleByDriverId: " + ex.Message);
            }
        }
        public async Task<List<VehicleLicenscePlateDTOs>> getVehicleByDriverId(int driverId)
        {
            if (driverId == null)
            {
                throw new ArgumentNullException(nameof(driverId), "Driver ID cannot be null.");
            }

            if (driverId <= 0)
            {
                throw new Exception("Driver ID must be greater than 0.");
            }

            try
            {
                var vehicleByDriverID = await _context.Vehicles.Include(v => v.VehicleType)
                    .Where(x => x.DriverId == driverId)
                    .Select(x => new VehicleLicenscePlateDTOs
                    {
                        Id = x.Id,
                        LicensePlate = x.LicensePlate,
                        NumberSeat = x.NumberSeat,
                        VehicleTypeName = x.VehicleType.Description,
                        DriverName = x.Driver.Name,
                        Status = x.Status,
                        Description = x.Description
                    }).ToListAsync();

                if (vehicleByDriverID == null || !vehicleByDriverID.Any())
                {
                    throw new Exception("Driver does not have any vehicle.");
                }

                return vehicleByDriverID;
            }
            catch (Exception ex)
            {
                throw new Exception("getVehicleByDriverId: " + ex.Message);
            }
        }
        public async Task<bool> checkDriver(int vehicleId, int driverId)
        {
            if (vehicleId == null)
            {
                throw new ArgumentNullException(nameof(vehicleId), "Vehicle ID cannot be null.");
            }

            if (vehicleId <= 0)
            {
                throw new Exception("Vehicle ID must be greater than 0.");
            }

            if (driverId == null)
            {
                throw new ArgumentNullException(nameof(driverId), "Driver ID cannot be null.");
            }

            if (driverId <= 0)
            {
                throw new Exception("Driver ID must be greater than 0.");
            }

            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                if (vehicle == null)
                {
                    throw new Exception("Vehicle not found.");
                }

                return vehicle.DriverId == driverId;
            }
            catch (Exception ex)
            {
                throw new Exception("checkDriver: " + ex.Message);
            }
        }
        public async Task<bool> UpdateVehicleAsync(int id, VehicleUpdateDTO updateDTO)
        {
            try
            {
                var licensePlatePattern = @"^\d{2}[A-Z]-\d{5}$";
                if (updateDTO.NumberSeat == null || updateDTO.NumberSeat <= 0)
                {
                    throw new Exception("NumberSeat must be greater than 0.");
                }
                if (string.IsNullOrWhiteSpace(updateDTO.LicensePlate))
                {
                    throw new Exception("License plate cannot be null or empty.");
                }
                if (!Regex.IsMatch(updateDTO.LicensePlate, licensePlatePattern))
                {
                    throw new Exception("Invalid license plate format. Expected format: 99A-99999.");
                }
                if (updateDTO.VehicleTypeId != Constant.XE_TIEN_CHUYEN && updateDTO.VehicleTypeId != Constant.XE_DU_LICH && updateDTO.VehicleTypeId != Constant.XE_LIEN_TINH)
                {
                    throw new Exception("Invalid type of vehicle");
                }
                var checkUserId = await _context.Drivers.SingleOrDefaultAsync(s => s.Id.Equals(updateDTO.DriverId));
                if (checkUserId == null && updateDTO.DriverId != null)
                {
                    throw new Exception("Not found driver");
                }
                var checkLicensePlate = await _context.Vehicles
                                    .Where(s => s.LicensePlate.Equals(updateDTO.LicensePlate) && s.Id != id)
                                    .FirstOrDefaultAsync();
                if (checkLicensePlate != null)
                {
                    throw new Exception("License plate is duplicate.");
                }

                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    throw new Exception("Not found vehicle");
                }

                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                var vehicleOwners = await _context.Users
                                    .Include(u => u.UserRoles)
                                    .ThenInclude(ur => ur.Role)
                                    .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "VehicleOwner"))
                                    .ToListAsync();
                bool isValidVehicleOwner = false;

                foreach (var user in vehicleOwners)
                {
                    if (user.Id == updateDTO.VehicleOwner)
                    {
                        isValidVehicleOwner = true;
                        break;
                    }
                }
                if (!isValidVehicleOwner)
                {
                    throw new Exception("The specified user does not have the role 'VehicleOwner'.");
                }

                if (updateDTO.NumberSeat.HasValue) vehicle.NumberSeat = updateDTO.NumberSeat.Value;
                if (updateDTO.VehicleTypeId.HasValue) vehicle.VehicleTypeId = updateDTO.VehicleTypeId.Value;
                if (updateDTO.Status.HasValue) vehicle.Status = updateDTO.Status.Value;
                if (!string.IsNullOrEmpty(updateDTO.Image)) vehicle.Image = updateDTO.Image;
                vehicle.DriverId = (updateDTO.DriverId != null) ? updateDTO.DriverId.Value : null;
                if (updateDTO.VehicleOwner.HasValue) vehicle.VehicleOwner = updateDTO.VehicleOwner.Value;
                if (!string.IsNullOrEmpty(updateDTO.LicensePlate)) vehicle.LicensePlate = updateDTO.LicensePlate;
                if (!string.IsNullOrEmpty(updateDTO.Description)) vehicle.Description = updateDTO.Description;

                vehicle.UpdateAt = DateTime.UtcNow;
                vehicle.UpdateBy = userId;


                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<VehicleLicenscePlateDTOs>> getVehicleByVehicleOwner(int vehicleOwner)
        {
            if (vehicleOwner == null)
            {
                throw new ArgumentNullException(nameof(vehicleOwner), "Vehicle owner ID cannot be null.");
            }

            if (vehicleOwner <= 0)
            {
                throw new Exception("Vehicle owner ID must be greater than 0.");
            }

            try
            {
                var listVehicle = await _context.Vehicles
                    .Where(x => x.VehicleOwner == vehicleOwner)
                    .Select(x => new VehicleLicenscePlateDTOs
                    {
                        Id = x.Id,
                        LicensePlate = x.LicensePlate,
                        NumberSeat = x.NumberSeat,
                        VehicleTypeName = _context.VehicleTypes
                    .Where(vt => vt.Id == x.VehicleTypeId)
                    .Select(vt => vt.Description)
                    .FirstOrDefault(),
                        DriverName = _context.Drivers
                    .Where(d => d.Id == x.DriverId)
                    .Select(d => d.Name)
                    .FirstOrDefault(),
                        Status = x.Status,
                        Description = x.Description
                    })
                    .ToListAsync();

                if (listVehicle == null || !listVehicle.Any())
                {
                    throw new Exception("No vehicles found for the given owner ID.");
                }

                return listVehicle;
            }
            catch (Exception ex)
            {
                throw new Exception("getVehicleByVehicleOwner: " + ex.Message);
            }
        }
        public async Task<(bool IsSuccess, List<ValidationErrorDTO> Errors)> ConfirmAddValidEntryImportVehicle(List<VehicleImportDTO> validEntries)
        {
            var errors = new List<ValidationErrorDTO>(); // Danh sách lỗi

            try
            {
                if (validEntries == null || validEntries.Count == 0)
                {
                    errors.Add(new ValidationErrorDTO
                    {
                        Row = 0,
                        ErrorMessage = "Empty data to import."
                    });
                    return (false, errors);
                }

                var vehicleOwners = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "VehicleOwner"))
                    .Select(u => new { u.Id, u.Username })
                    .ToListAsync();

                var vehicleOwnerSet = new HashSet<int>(vehicleOwners.Select(v => v.Id));
                var existingLicensePlates = await _context.Vehicles
                                            .Select(v => v.LicensePlate)
                                            .ToListAsync();
                var licensePlateSet = new HashSet<string>(existingLicensePlates);
                int rowEntries = 1;
                var drivers = await _context.Drivers
                            .Select(d => d.Id)
                            .ToListAsync();

                var driverSet = new HashSet<int>(drivers);

                foreach (var entry in validEntries)
                {
                    var licensePlatePattern = @"^\d{2}[A-Z]-\d{5}$";
                    entry.Status = true;
                    if (entry.DriverId == 0)
                    {
                        entry.DriverId = null;
                    }
                    if (entry.NumberSeat == null || entry.NumberSeat <= 0)
                    {
                        errors.Add(new ValidationErrorDTO
                        {
                            Row = rowEntries,
                            ErrorMessage = "NumberSeat must be greater than 0."
                        });
                    }

                    if (string.IsNullOrWhiteSpace(entry.LicensePlate))
                    {
                        errors.Add(new ValidationErrorDTO
                        {
                            Row = rowEntries,
                            ErrorMessage = "License plate cannot be null or empty."
                        });
                    }
                    else
                    {
                        if (!Regex.IsMatch(entry.LicensePlate, licensePlatePattern))
                        {
                            errors.Add(new ValidationErrorDTO
                            {
                                Row = rowEntries,
                                ErrorMessage = "Invalid license plate format. Expected format: 99A-99999."
                            });
                        }
                        // Kiểm tra trùng lặp với cơ sở dữ liệu
                        if (licensePlateSet.Contains(entry.LicensePlate))
                        {
                            errors.Add(new ValidationErrorDTO
                            {
                                Row = rowEntries,
                                ErrorMessage = "Duplicate LicensePlate found in database."
                            });
                        }
                    }

                    if (entry.VehicleTypeId != Constant.XE_TIEN_CHUYEN &&
                    entry.VehicleTypeId != Constant.XE_DU_LICH &&
                    entry.VehicleTypeId != Constant.XE_LIEN_TINH)
                    {
                        errors.Add(new ValidationErrorDTO
                        {
                            Row = rowEntries,
                            ErrorMessage = "Invalid type of vehicle."
                        });
                    }

                    if (entry.VehicleOwner == null || !vehicleOwnerSet.Contains(entry.VehicleOwner.Value))
                    {
                        errors.Add(new ValidationErrorDTO
                        {
                            Row = rowEntries,
                            ErrorMessage = "Invalid VehicleOwner. The specified user does not have the role 'VehicleOwner'."
                        });
                    }
                    rowEntries++;
                }

                if (errors.Count > 0)
                {
                    return (false, errors);
                }
                var listMapper = _mapper.Map<List<Vehicle>>(validEntries);
                await _context.AddRangeAsync(listMapper);
                await _context.SaveChangesAsync();

                return (true, errors); // Không có lỗi, trả về thành công
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationErrorDTO
                {
                    Row = 0,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                });
                return (false, errors);
            }
        }
        public async Task<List<VehicleBasicDto>> GetAvailableVehiclesAsync()
        {
            try
            {
                var vehicles = await (from v in _context.Vehicles
                                      where !_context.VehicleTrips.Any(vt => vt.VehicleId == v.Id)
                                      select v).ToListAsync();

                return _mapper.Map<List<VehicleBasicDto>>(vehicles);
            }
            catch (Exception ex)
            {
                throw new Exception("GetAvailableVehiclesAsync: " + ex.Message);
            }
        }



    }
}