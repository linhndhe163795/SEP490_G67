using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;


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

        public async Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO, string driverName)
        {
            try
            {
                if (vehicleAddDTO == null)
                {
                    throw new ArgumentNullException(nameof(vehicleAddDTO), "VehicleAddDTO cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(driverName))
                {
                    throw new ArgumentException("Driver name cannot be null or empty.");
                }

                if (vehicleAddDTO.NumberSeat == null || vehicleAddDTO.NumberSeat <= 0)
                {
                    throw new ArgumentException("NumberSeat must be greater than 0.");
                }

                if (string.IsNullOrWhiteSpace(vehicleAddDTO.LicensePlate))
                {
                    throw new ArgumentException("License plate cannot be null or empty.");
                }

                if (vehicleAddDTO.CreatedAt == null || vehicleAddDTO.CreatedAt > DateTime.Now)
                {
                    throw new ArgumentException("CreatedAt must be a valid date and cannot be in the future.");
                }

                var checkUserNameDrive = await _context.Drivers.SingleOrDefaultAsync(s => s.UserName.Equals(driverName));

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

                bool isStaff = user.UserRoles.Any(s => s.Role.RoleName == "Staff");

                Vehicle vehicle = new Vehicle
                {
                    NumberSeat = vehicleAddDTO.NumberSeat.Value,
                    VehicleTypeId = vehicleAddDTO.VehicleTypeId ?? 1,
                    Image = vehicleAddDTO.Image,
                    Status = isStaff,
                    DriverId = checkUserNameDrive != null ? checkUserNameDrive.Id : 0,
                    VehicleOwner = userId,
                    LicensePlate = vehicleAddDTO.LicensePlate,
                    Description = vehicleAddDTO.Description,
                    CreatedBy = userId,
                    CreatedAt = vehicleAddDTO.CreatedAt.Value,
                    UpdateAt = vehicleAddDTO.UpdateAt ?? DateTime.Now,
                    UpdateBy = vehicleAddDTO.UpdateBy ?? userId,
                };

                _context.Vehicles.Add(vehicle);

                if (!isStaff)
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
                if (requestId <= 0)
                {
                    throw new ArgumentException("Request ID must be greater than 0.");
                }

                var checkRequestExits = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);

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

                if (checkRequestExits == null)
                {
                    throw new Exception("Request does not exist.");
                }

                checkRequestExits.Note = isApprove ? "Đã xác nhận" : "Từ chối xác nhận";
                checkRequestExits.Status = isApprove;
                var updateRequest = await _requestRepository.UpdateRequestVehicleAsync(requestId, checkRequestExits);

                if (!updateRequest)
                {
                    throw new Exception("Failed to update request.");
                }

                var emailOfUser = await _context.Users
                                                .Where(rq => rq.Id == checkRequestExits.UserId)
                                                .Select(rq => rq.Email)
                                                .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(emailOfUser))
                {
                    throw new Exception("User email not found.");
                }

                SendMailDTO mail = new SendMailDTO
                {
                    FromEmail = "nhaxenhanam@gmail.com",
                    Password = "vzgq unyk xtpt xyjp",
                    ToEmail = emailOfUser,
                    Subject = "Thông báo về việc đăng ký xe vào hệ thống",
                    Body = isApprove ? "Hệ thống đã xác nhận yêu cầu xe của bạn. Xe của bạn đã tham gia hệ thống."
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

                if (vehicleID == 0)
                {
                    throw new Exception("Vehicle ID not found in request details.");
                }

                UpdateVehicleByStaff(vehicleID.Value, user.Id, isApprove);

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
                    throw new ArgumentException("Vehicle ID must be greater than 0.");
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
                    throw new ArgumentException("Vehicle ID must be greater than 0.");
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
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var vehicleUpdate = await _context.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);

                var checkUserNameDrive = _context.Drivers.SingleOrDefault(s => s.Name.Equals(driverName));

                if (vehicleUpdate != null && checkUserNameDrive != null)
                {
                    vehicleUpdate.DriverId = userId;

                    vehicleUpdate.UpdateBy = checkUserNameDrive.Id;

                    vehicleUpdate.UpdateAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return true;

                }
                else
                {
                    throw new Exception("Not found user name in system");
                }
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
                var i = 1;
                var listStartPoint = await (from v in _context.Vehicles
                                            join vt in _context.VehicleTrips
                                            on v.Id equals vt.VehicleId
                                            join t in _context.Trips
                                            on vt.TripId equals t.Id
                                            where v.Id == vehicleId
                                            select t.PointStart).Distinct()
                                         .ToListAsync();
                List<StartPointDTO> listStartPointDTOs = new List<StartPointDTO>();
                foreach (var v in listStartPoint)
                {
                    listStartPointDTOs.Add(new StartPointDTO { id = i++, name = v });
                }

                if (listStartPointDTOs == null)
                {
                    throw new ArgumentNullException(nameof(listStartPointDTOs));
                }
                else
                {
                    return listStartPointDTOs;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetListStartPointByVehicleId: " + ex.Message);
            }
        }

        public async Task<List<VehicleListDTO>> GetVehicleDTOsAsync()
        {
            try
            {
                var listVehicle = await _context.Vehicles.ToListAsync();

                var vehicleListDTOs = _mapper.Map<List<VehicleListDTO>>(listVehicle);

                return vehicleListDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetVehicleDTOsAsync: " + ex.Message);
            }
        }

        public async Task<bool> UpdateVehicleAsync(int id, string driverName, int userIdUpdate)
        {
            try
            {
                var vehicleUpdate = await _context.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);

                var checkUserNameDrive = _context.Drivers.SingleOrDefault(s => s.Name.Equals(driverName));

                if (vehicleUpdate != null && checkUserNameDrive != null)
                {
                    vehicleUpdate.DriverId = userIdUpdate;

                    vehicleUpdate.UpdateBy = checkUserNameDrive.Id;

                    vehicleUpdate.UpdateAt = DateTime.Now;

                    //await _context.SaveChangesAsync();

                    return true;

                }
                else
                {
                    throw new Exception("Not found user name in system");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateVehicleAsync: " + ex.Message);
            }
        }

        private bool UpdateVehicleByStaff(int id, int userIdUpdate, bool updateStatus)
        {
            try
            {
                var vehicleUpdate = _context.Vehicles.SingleOrDefault(vehicle => vehicle.Id == id);


                if (vehicleUpdate != null)
                {
                    vehicleUpdate.Status = updateStatus;

                    vehicleUpdate.UpdateBy = userIdUpdate;

                    vehicleUpdate.UpdateAt = DateTime.Now;

                    _context.Vehicles.Update(vehicleUpdate);

                    _context.SaveChanges();

                    return true;

                }
                else
                {
                    throw new Exception("Not found vehicle in system");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateVehicleByStaff: " + ex.Message);
            }
        }

        public async Task<bool> AssignDriverToVehicleAsync(int vehicleId, int driverId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null || driverId <= 0)
            {
                return false;
            }

            vehicle.DriverId = driverId;
            _context.Vehicles.Update(vehicle);

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<int> GetNumberSeatAvaiable(int tripId, DateTime dateTime)
        {
            try
            {
                var ticketCount = await _tripRepository.GetTicketCount(tripId, dateTime);
                var vehicleTrip = await _context.VehicleTrips.FirstOrDefaultAsync(x => x.TripId == tripId);
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleTrip.VehicleId);
                var seatAvaiable = vehicle.NumberSeat - ticketCount;
                return seatAvaiable.Value;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<VehicleAddDTO> GetVehicleById(int vehicleId)
        {
            try
            {
                var vehicleById = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                var vehicleMapper = _mapper.Map<VehicleAddDTO>(vehicleById);
                return vehicleMapper;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
            try
            {
                var vehicleId = await _context.Vehicles.FirstOrDefaultAsync(x => x.DriverId == driverId);
                if (vehicleId == null)
                {
                    throw new Exception("Driver not have vehicle");
                }
                return vehicleId.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<List<VehicleLicenscePlateDTOs>> getVehicleByDriverId(int driverId)
        {
            try
            {
                var vehicleByDriverID = _context.Vehicles.Where(x => x.DriverId == driverId).Select(x => new VehicleLicenscePlateDTOs
                {
                    Id = x.Id,
                    LicensePlate = x.LicensePlate
                }).ToListAsync();
                if (vehicleByDriverID == null)
                {
                    throw new Exception("Driver not have vehicle");
                }
                return vehicleByDriverID;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> checkDriver(int vehicleId, int driverId)
        {
            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                if (vehicle.DriverId == driverId)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateVehicleAsync(int id, VehicleUpdateDTO updateDTO)
        {
            try
            {

                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null) return false;


                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }


                if (updateDTO.NumberSeat.HasValue) vehicle.NumberSeat = updateDTO.NumberSeat.Value;
                if (updateDTO.VehicleTypeId.HasValue) vehicle.VehicleTypeId = updateDTO.VehicleTypeId.Value;
                if (updateDTO.Status.HasValue) vehicle.Status = updateDTO.Status.Value;
                if (!string.IsNullOrEmpty(updateDTO.Image)) vehicle.Image = updateDTO.Image;
                if (updateDTO.DriverId.HasValue) vehicle.DriverId = updateDTO.DriverId.Value;
                if (updateDTO.VehicleOwner.HasValue) vehicle.VehicleOwner = updateDTO.VehicleOwner.Value;
                if (!string.IsNullOrEmpty(updateDTO.LicensePlate)) vehicle.LicensePlate = updateDTO.LicensePlate;
                if (!string.IsNullOrEmpty(updateDTO.Description)) vehicle.Description = updateDTO.Description;

                vehicle.UpdateAt = DateTime.UtcNow;
                vehicle.UpdateBy = userId;


                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (ArgumentException ex)
            {

                throw new Exception($"Validation error: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {

                throw new Exception("Database update error. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {

                throw new Exception("An unexpected error occurred while updating the vehicle.", ex);
            }
        }

        public async Task<List<VehicleLicenscePlateDTOs>> getVehicleByVehicleOwner(int vehicleOwner)
        {
            try
            {
                var listVehicle = await _context.Vehicles.Where(x => x.VehicleOwner == vehicleOwner).Select(x => new VehicleLicenscePlateDTOs
                {
                    Id = x.Id,
                    LicensePlate = x.LicensePlate
                }).ToListAsync();
                if(listVehicle.Count <= 0)
                {
                    throw new Exception("Not found vehicleId");
                }
                return listVehicle;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

