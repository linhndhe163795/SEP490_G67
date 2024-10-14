using Microsoft.AspNetCore.Mvc;
using MyAPI.Models;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.DTOs.DriverDTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyAPI.DTOs.DriverDTOs;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverRepository _driverRepository;
        ITypeOfDriverRepository _typeOfDriverRepository;
        private readonly IMapper _mapper;

        public DriverController(IDriverRepository driverRepository, ITypeOfDriverRepository  typeOfDriverRepository, IMapper mapper)
        {
            _driverRepository = driverRepository;
            _typeOfDriverRepository = typeOfDriverRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            var drivers = await _driverRepository.GetAll();
            var UpdateDriverDtos = _mapper.Map<IEnumerable<DriverDTO>>(drivers);
            return Ok(UpdateDriverDtos);
        }

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

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] UpdateDriverDTO updateDriverDto)
        {
            if (updateDriverDto == null)
            {
                return BadRequest("Invalid driver data");
            }

            var driver = _mapper.Map<Driver>(updateDriverDto);

            
            var typeOfDriver = await _typeOfDriverRepository.Get(updateDriverDto.TypeOfDriver);
            if (typeOfDriver == null)
            {
                return BadRequest("Invalid TypeOfDriver ID");
            }

            driver.TypeOfDriver = typeOfDriver.Id;

            await _driverRepository.Add(driver);
            var createdDriverDto = _mapper.Map<UpdateDriverDTO>(driver);
            return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, createdDriverDto);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDTO UpdateDriverDto)
        {
            if (UpdateDriverDto == null)
            {
                return BadRequest("Invalid driver data");
            }

            var existingDriver = await _driverRepository.Get(id);
            if (existingDriver == null)
            {
                return NotFound("Driver not found");
            }

            
            existingDriver.UserName = UpdateDriverDto.UserName;
            existingDriver.Name = UpdateDriverDto.Name;
            existingDriver.NumberPhone = UpdateDriverDto.NumberPhone;
            existingDriver.Avatar = UpdateDriverDto.Avatar;
            existingDriver.Dob = UpdateDriverDto.Dob;
            existingDriver.StatusWork = UpdateDriverDto.StatusWork;
            existingDriver.TypeOfDriver = UpdateDriverDto.TypeOfDriver;
            existingDriver.Status = UpdateDriverDto.Status;
            existingDriver.VehicleId = UpdateDriverDto.VehicleId; 

            existingDriver.UpdateAt = DateTime.UtcNow;

            await _driverRepository.Update(existingDriver);

            return Ok("Driver updated successfully");
        }


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
    }
}
