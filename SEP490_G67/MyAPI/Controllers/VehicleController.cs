using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {

        private readonly IVehicleRepository _vehicleRepository;
        public VehicleController(IVehicleRepository vehicleRepository, IMapper mapper)
        {
            _vehicleRepository = vehicleRepository;
        }

        [HttpPost("addVehicle")]
        public async Task<IActionResult> AddVehicle(VehicleAddDTO vehicleAddDTO, string driverName)
        {
            try
            {
               var checkAdd = await _vehicleRepository.AddVehicleAsync(vehicleAddDTO, driverName);
                if (checkAdd == true)
                {
                    return Ok(vehicleAddDTO);
                }else
                {
                    return NotFound(new { Message = "Driver not found. Please check the driver name." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle failed", Details = ex.Message });
            }

        }

        [HttpPost("updateVehicle")]
        public async Task<IActionResult> UpdateVehicle(int id, string driverName, int userIdUpdate)
        {
            try
            {
                var checkUpdate = await _vehicleRepository.UpdateVehicleAsync(id, driverName, userIdUpdate);
                if (checkUpdate == true)
                {
                    return Ok();
                }
                else
                {
                    return NotFound(new { Message = "Driver name not found. Please check the driver name." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "UpdateVehicle failed", Details = ex.Message });
            }

        }
    }
}
