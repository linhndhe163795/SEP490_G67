using Microsoft.AspNetCore.Mvc;
using MyAPI.Models;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.DTOs.DriverDTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverRepository _driverRepository;
        ITypeOfDriverRepository _typeOfDriverRepository;
        private readonly IMapper _mapper;

        public DriverController(IDriverRepository driverRepository, ITypeOfDriverRepository typeOfDriverRepository, IMapper mapper)
        {
            _driverRepository = driverRepository;
            _typeOfDriverRepository = typeOfDriverRepository;
            _mapper = mapper;
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            var drivers = await _driverRepository.GetAll();
            var UpdateDriverDtos = _mapper.Map<IEnumerable<DriverDTO>>(drivers);
            return Ok(UpdateDriverDtos);
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            var driver = await _driverRepository.Get(id);
            if (driver == null)
            {
                return NotFound("Driver not found");
            }

            var UpdateDriverDto = _mapper.Map<DriverDTO>(driver);
            return Ok(UpdateDriverDto);
        }
        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] UpdateDriverDTO updateDriverDto)
        {
            if (updateDriverDto == null)
            {
                return BadRequest("Invalid driver data");
            }

            try
            {
                var driver = await _driverRepository.CreateDriverAsync(updateDriverDto);
                var createdDriverDto = _mapper.Map<UpdateDriverDTO>(driver);
                return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, createdDriverDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDTO updateDriverDto)
        {
            if (updateDriverDto == null)
            {
                return BadRequest("Invalid driver data");
            }

            try
            {
                var existingDriver = await _driverRepository.UpdateDriverAsync(id, updateDriverDto);
                return Ok(existingDriver);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var driver = await _driverRepository.Get(id);
            if (driver == null)
            {
                return NotFound("Driver not found");
            }

            await _driverRepository.Delete(driver);
            return Ok("Driver deleted successfully");
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("drivers-without-vehicle-for-rent")]
        public async Task<IActionResult> GetDriversWithoutVehicle()
        {
            var drivers = await _driverRepository.GetDriversWithoutVehicleAsync();
            return Ok(drivers);
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("send-mail-to-drivers-without-vehicle-for-rent")]
        public async Task<IActionResult> SendMailToDriverWithoutVehicle(int price)
        {
            try
            {
                await _driverRepository.SendEmailToDriversWithoutVehicle(price);

                return Ok("Emails sent successfully to all drivers without vehicles.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while sending emails: " + ex.Message);
            }
        }
    }
}
