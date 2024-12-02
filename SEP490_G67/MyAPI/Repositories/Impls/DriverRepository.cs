using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MyAPI.Repositories.Impls
{
    public class DriverRepository : GenericRepository<Driver>, IDriverRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly ITypeOfDriverRepository _typeOfDriverRepository;
        private readonly SendMail _sendMail;
        private readonly HashPassword _hashPassword;
        private readonly IMapper _mapper;


        public DriverRepository(SEP490_G67Context context, ITypeOfDriverRepository typeOfDriverRepository
            , SendMail sendMail, HashPassword hashPassword, IMapper mapper) : base(context)
        {
            _context = context;
            _typeOfDriverRepository = typeOfDriverRepository;
            _sendMail = sendMail;
            _hashPassword = hashPassword;
            _mapper = mapper;
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
            try
            {
                var checkUserName = await _context.Drivers.FirstOrDefaultAsync(x => x.UserName == updateDriverDto.UserName);

                if (checkUserName != null)
                {
                    throw new Exception("User Name is exist in system");
                }
                if (updateDriverDto.NumberPhone == null)
                {
                    throw new ArgumentNullException(nameof(updateDriverDto.NumberPhone), "NumberPhone cannot be null");
                }
                if (updateDriverDto.Password == null)
                {
                    throw new ArgumentNullException(nameof(updateDriverDto.Password), "Password cannot be null");
                }
                if (updateDriverDto.UserName == null)
                {
                    throw new ArgumentNullException(nameof(updateDriverDto.UserName), "UserName cannot be null");
                }
                var hashPassword = _hashPassword.HashMD5Password(updateDriverDto.Password);
                var driver = new Driver
                {
                    UserName = updateDriverDto.UserName,
                    Name = updateDriverDto.Name,
                    NumberPhone = updateDriverDto.NumberPhone,
                    Avatar = updateDriverDto.Avatar,
                    Dob = updateDriverDto.Dob,
                    License = updateDriverDto.License,
                    Password = hashPassword,
                    StatusWork = "Active",
                    Status = updateDriverDto.Status,
                    TypeOfDriver = 1
                };
                await Add(driver);
                return driver;
            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        public async Task<Driver> UpdateDriverAsync(int id, UpdateDriverDTO updateDriverDto)
        {
            if (updateDriverDto == null)
            {
                throw new ArgumentNullException(nameof(updateDriverDto), "Update data is required.");
            }

            var existingDriver = await Get(id);
            if (existingDriver == null)
            {
                throw new Exception("Driver not found");
            }
            if (string.IsNullOrWhiteSpace(updateDriverDto.Name))
            {
                throw new Exception("Name cannot be null or empty.");
            }
            if (!string.IsNullOrEmpty(updateDriverDto.NumberPhone) && !Regex.IsMatch(updateDriverDto.NumberPhone, @"^\d+$"))
            {
                throw new Exception("NumberPhone must contain only digits.");
            }
            existingDriver.Name = updateDriverDto.Name;
            existingDriver.NumberPhone = updateDriverDto.NumberPhone;
            existingDriver.Avatar = updateDriverDto.Avatar;
            existingDriver.Dob = updateDriverDto.Dob;
            existingDriver.License = updateDriverDto.License;
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

        public async Task<bool> checkLogin(LoginDriverDTO login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login), "Login data is required.");
            }

            if (string.IsNullOrWhiteSpace(login.UserName) && string.IsNullOrWhiteSpace(login.Email))
            {
                throw new Exception("Either UserName or Email must be provided.");
            }

            if (string.IsNullOrWhiteSpace(login.Password))
            {
                throw new Exception("Password is required.");
            }
            try
            {
                var hashPassword = _hashPassword.HashMD5Password(login.Password);
                var driver = await _context.Drivers.FirstOrDefaultAsync((x => (x.UserName == login.UserName || x.Email == login.Email) && x.Password == hashPassword));
                return driver != null ? true : false;

            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<DriverLoginRespone> getDriverLogin(LoginDriverDTO login)
        {
            try
            {
                var hashPassword = _hashPassword.HashMD5Password(login.Password);
                var driver = await _context.Drivers.FirstOrDefaultAsync((x => (x.UserName == login.UserName || x.Email == login.Email) && x.Password == hashPassword));
                var driverLoginRespone = new DriverLoginRespone
                {
                    Email = driver.Email,
                    UserName = driver.UserName,
                    Id = driver.Id,
                    RoleName = "Driver"

                };
                return driverLoginRespone;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ListDriverDTO>> getListDriverForVehicle()
        {
            var listDriver = await _context.Drivers.ToListAsync();

            var driveListDTOs = _mapper.Map<List<ListDriverDTO>>(listDriver);

            return driveListDTOs;
        }
    }
}
