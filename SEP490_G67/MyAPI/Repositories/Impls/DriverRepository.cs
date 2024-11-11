using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Globalization;

namespace MyAPI.Repositories.Impls
{
    public class DriverRepository : GenericRepository<Driver>, IDriverRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly ITypeOfDriverRepository _typeOfDriverRepository;
        private readonly SendMail _sendMail;

        public DriverRepository(SEP490_G67Context context, ITypeOfDriverRepository typeOfDriverRepository, SendMail sendMail) : base(context)
        {
            _context = context;
            _typeOfDriverRepository = typeOfDriverRepository;
            _sendMail = sendMail;
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
            existingDriver.UpdateAt = DateTime.UtcNow;

            await Update(existingDriver);
            return existingDriver;
        }

        public async Task<IEnumerable<Driver>> GetDriversWithoutVehicleAsync()
        {
            return await _context.Drivers
                .Where(d => !_context.Vehicles.Any(v => v.DriverId == d.Id))
                .ToListAsync();
        }

        

        public async Task SendEmailToDriversWithoutVehicle(int price)
        {
        try
        {
            var driversWithoutVehicle = await GetDriversWithoutVehicleAsync();

            if (!driversWithoutVehicle.Any())
            {
                Console.WriteLine("No drivers without vehicles found.");
                return;
            }

            string formattedPrice = string.Format(new CultureInfo("vi-VN"), "{0:N0} VND", price);

            foreach (var driver in driversWithoutVehicle)
            {
                if (string.IsNullOrWhiteSpace(driver.Email))
                {
                    Console.WriteLine($"Driver {driver.Name} does not have an email address. Skipping...");
                    continue;
                }

                SendMailDTO sendMailDTO = new()
                {
                    FromEmail = "duclinh5122002@gmail.com",
                    Password = "jetj haze ijdw euci",
                    ToEmail = driver.Email,
                    Subject = "Vehicle Rental Opportunity",
                    Body = $"Hello {driver.Name},\n\nWe currently have vehicles available and would like to hire you at a rate of {formattedPrice}. Please contact us if you are interested in renting a vehicle.\n\nBest regards,\nYour Company Name"
                };

                if (!await _sendMail.SendEmail(sendMailDTO))
                {
                    Console.WriteLine($"Failed to send email to driver {driver.Email}.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("SendEmailToDriversWithoutVehicle error: " + ex.Message);
            throw new Exception("Failed to send email to drivers without vehicles.", ex);
        }
    }



}
}
