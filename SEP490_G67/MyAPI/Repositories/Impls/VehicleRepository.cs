using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace MyAPI.Repositories.Impls
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly IRequestRepository _requestRepository;
        private readonly IRequestDetailRepository _requestDetailRepository;


        public VehicleRepository(SEP490_G67Context context, IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            GetInforFromToken tokenHelper, 
            IRequestRepository requestRepository,
            IRequestDetailRepository requestDetailRepository) : base(context)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _requestRepository = requestRepository;
            _requestDetailRepository = requestDetailRepository;
        }

        public async Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO, string driverName)
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

            if (checkUserNameDrive == null)
            {
                throw new Exception("Driver not found in the system.");
            }

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
                DriverId = checkUserNameDrive.Id,
                VehicleOwner = userId,
                LicensePlate = vehicleAddDTO.LicensePlate,
                Description = vehicleAddDTO.Description,
                CreatedBy = vehicleAddDTO.CreatedBy,
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

                var createdRequest = await _requestRepository.CreateRequestAsync(requestDTO);
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
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddVehicleByStaffcheckAsync(int requestId, bool isApprove)
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
            var updateRequest = await _requestRepository.UpdateRequestAsync(requestId, checkRequestExits);

            if (updateRequest == null)
            {
                throw new Exception("Failed to update request.");
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


        public async Task<bool> DeleteVehicleAsync(int id)
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

        public async Task<List<VehicleListDTO>> GetVehicleDTOsAsync()
        {
            var listVehicle = _context.Vehicles.ToList();

            var vehicleListDTOs = _mapper.Map<List<VehicleListDTO>>(listVehicle);

            return vehicleListDTOs;
        }

        public async Task<List<VehicleTypeDTO>> GetVehicleTypeDTOsAsync()
        {
            var listVehicleType = _context.VehicleTypes.ToList();

            var vehicleTypeListDTOs = _mapper.Map<List<VehicleTypeDTO>>(listVehicleType);

            return vehicleTypeListDTOs;
        }

        public async Task<bool> UpdateVehicleAsync(int id, string driverName)
        {

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }

            var vehicleUpdate = await _context.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);

            var checkUserNameDrive =  _context.Drivers.SingleOrDefault(s => s.Name.Equals(driverName));

            if (vehicleUpdate != null && checkUserNameDrive != null)
            {
                vehicleUpdate.DriverId = userId;

                vehicleUpdate.UpdateBy = checkUserNameDrive.Id;

                vehicleUpdate.UpdateAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;

            }else
            {
                throw new Exception("Not found user name in system");
            }
        }


        private bool UpdateVehicleByStaff(int id, int userIdUpdate, bool updateStatus)
        {
            var vehicleUpdate =  _context.Vehicles.SingleOrDefault(vehicle => vehicle.Id == id);


            if (vehicleUpdate != null )
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



    }
}
