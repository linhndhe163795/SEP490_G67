using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class DriverRepository : GenericRepository<Driver>, IDriverRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly ITypeOfDriverRepository _typeOfDriverRepository;

        public DriverRepository(SEP490_G67Context context, ITypeOfDriverRepository typeOfDriverRepository) : base(context)
        {
            _context = context;
            _typeOfDriverRepository = typeOfDriverRepository;
        }

        public async Task<Driver> GetDriverWithVehicle(int id)
        {
            return await _context.Drivers.Include(d => d.Vehicles).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<int> lastIdDriver()
        {
            int lastId = await _context.Drivers.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
            return lastId;
        }

        public async Task<Driver> CreateDriverAsync(UpdateDriverDTO updateDriverDto)
        {
            var driver = new Driver
            {
                UserName = updateDriverDto.UserName,
                Name = updateDriverDto.Name,
                NumberPhone = updateDriverDto.NumberPhone,
                Avatar = updateDriverDto.Avatar,
                Dob = updateDriverDto.Dob,
                StatusWork = updateDriverDto.StatusWork,
                Status = updateDriverDto.Status,
                VehicleId = updateDriverDto.VehicleId
            };

            var typeOfDriver = await _typeOfDriverRepository.Get(updateDriverDto.TypeOfDriver);
            if (typeOfDriver == null)
            {
                throw new ArgumentException("Invalid TypeOfDriver ID");
            }

            driver.TypeOfDriver = typeOfDriver.Id;

            await Add(driver);
            return driver;
        }

        public async Task<Driver> UpdateDriverAsync(int id, UpdateDriverDTO updateDriverDto)
        {
            var existingDriver = await Get(id);
            if (existingDriver == null)
            {
                throw new KeyNotFoundException("Driver not found");
            }

            existingDriver.UserName = updateDriverDto.UserName;
            existingDriver.Name = updateDriverDto.Name;
            existingDriver.NumberPhone = updateDriverDto.NumberPhone;
            existingDriver.Avatar = updateDriverDto.Avatar;
            existingDriver.Dob = updateDriverDto.Dob;
            existingDriver.StatusWork = updateDriverDto.StatusWork;
            existingDriver.TypeOfDriver = updateDriverDto.TypeOfDriver;
            existingDriver.Status = updateDriverDto.Status;
            existingDriver.VehicleId = updateDriverDto.VehicleId;
            existingDriver.UpdateAt = DateTime.UtcNow;

            await Update(existingDriver);
            return existingDriver;
        }
    }
}
