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
                var checkUserNameDrive = await _context.Drivers.SingleOrDefaultAsync(s => s.Name.Equals(driverName));

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

                //if (checkUserNameDrive == null)
                //{
                //    throw new Exception("Driver not found in the system.");
                //}

                var checkLicensePlate = await _context.Vehicles.FirstOrDefaultAsync(s => s.LicensePlate.Equals(vehicleAddDTO.LicensePlate));
                if (checkLicensePlate != null)
                {
                    throw new Exception("License plate is duplicate.");
                }

                bool isStaff = user.UserRoles.Any(s => s.Role.RoleName == "Staff");

                Vehicle vehicle = new Vehicle
                {
                    NumberSeat = vehicleAddDTO.NumberSeat,
                    VehicleTypeId = vehicleAddDTO.VehicleTypeId,
                    Image = vehicleAddDTO.Image,
                    Status = isStaff,
                    DriverId = checkUserNameDrive != null ? checkUserNameDrive.Id : 0,
                    VehicleOwner = userId,
                    LicensePlate = vehicleAddDTO.LicensePlate,
                    Description = vehicleAddDTO.Description,
                    CreatedBy = userId,
                    CreatedAt = vehicleAddDTO.CreatedAt,
                    UpdateAt = vehicleAddDTO.UpdateAt,
                    UpdateBy = vehicleAddDTO.UpdateBy,
                };

                _context.Vehicles.Add(vehicle);


                if (!isStaff)
                {
                    var requestDTO = new RequestDTO
                    {
                        UserId = userId,
                        TypeId = 1,
                        Description = "Yêu cầu thêm xe",
                        Note = "Đang chờ xác nhận",
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
                        Subject = "Thông báo về việc đăng ký xe vào hệ thống",
                        Body = "Thông tin của bạn đã được chúng tôi tiếp nhận xin vui lòng chờ đợi kiểm duyệt"
                    };

                    var checkMail = await _sendMail.SendEmail(mail);
                    if (!checkMail)
                    {
                        throw new Exception("Send mail fail!!");
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
                    return false;
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
                var checkVehicle = await _context.Vehicles.SingleOrDefaultAsync(s => s.Id == id);
                if (checkVehicle != null)
                {
                    checkVehicle.Status = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
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
                var i = 1;
                var listStartPoint = await (from v in _context.Vehicles
                                            join vt in _context.VehicleTrips
                                            on v.Id equals vt.VehicleId
                                            join t in _context.Trips
                                            on vt.TripId equals t.Id
                                            where v.Id == vehicleId
                                            select t.PointEnd).Distinct()
                                         .ToListAsync();
                List<EndPointDTO> listEndPointDTOs = new List<EndPointDTO>();
                foreach (var v in listStartPoint)
                {
                    listEndPointDTOs.Add(new EndPointDTO{ id = i++, name = v });
                }

                if (listEndPointDTOs == null)
                {
                    throw new ArgumentNullException(nameof(listEndPointDTOs));
                }
                else
                {
                    return listEndPointDTOs;
                }
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
            }catch (Exception ex)
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
            }catch (Exception ex)
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
            }catch (Exception ex)
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
            }catch(Exception ex)
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
        public async Task<int> GetNumberSeatAvaiable(int vehicleId)
        {
            try
            {
                var ticketCount = await _tripRepository.GetTicketCount(vehicleId);
                var vehicel = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                var seatAvaiable = vehicel.NumberSeat - ticketCount;
                return seatAvaiable.Value;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

