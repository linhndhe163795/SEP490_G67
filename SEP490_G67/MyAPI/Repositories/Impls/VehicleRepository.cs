using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly IMapper _mapper;

        
        public VehicleRepository(SEP490_G67Context context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<bool> AddVehicleAsync(VehicleAddDTO vehicleAddDTO, string driverName)
        {
            var checkUserNameDrive = _context.Drivers.SingleOrDefault(s => s.Name.Equals(driverName));
            if (checkUserNameDrive == null)
            {
                throw new Exception("Not found user name in system");
            }else
            {
                Vehicle vehicle = new Vehicle
                {
                    NumberSeat = vehicleAddDTO.NumberSeat,
                    VehicleTypeId = vehicleAddDTO.VehicleTypeId,
                    Status = vehicleAddDTO.Status,
                    DriverId = checkUserNameDrive.Id,
                    VehicleOwner = vehicleAddDTO.VehicleOwner,
                    LicensePlate = vehicleAddDTO.LicensePlate,
                    Description = vehicleAddDTO.Description,
                    CreatedBy = vehicleAddDTO.CreatedBy,
                    CreatedAt = vehicleAddDTO.CreatedAt,
                    UpdateAt = vehicleAddDTO.UpdateAt,
                    UpdateBy = vehicleAddDTO.UpdateBy,
                    
                };
                _context.Vehicles.Add(vehicle);
                //await _context.SaveChangesAsync();
                return true;
            }


        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var checkVehicle = await _context.Vehicles.SingleOrDefaultAsync(s => s.Id == id);
            if (checkVehicle != null)
            {
                checkVehicle.Status = false;
                //await _context.SaveChangesAsync();
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

        public async Task<bool> UpdateVehicleAsync(int id, string driverName, int userIdUpdate)
        {
            var vehicleUpdate = await _context.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);

            var checkUserNameDrive =  _context.Drivers.SingleOrDefault(s => s.Name.Equals(driverName));

            if (vehicleUpdate != null && checkUserNameDrive != null)
            {
                vehicleUpdate.DriverId = userIdUpdate;

                vehicleUpdate.UpdateBy = checkUserNameDrive.Id;

                vehicleUpdate.UpdateAt = DateTime.Now;

                //await _context.SaveChangesAsync();

                return true;

            }else
            {
                throw new Exception("Not found user name in system");
            }
        }
    }
}
