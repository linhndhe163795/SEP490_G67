using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {

        private readonly IVehicleRepository _vehicleRepository;
        public VehicleController(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("listVehicleType")]
        public async Task<IActionResult> GetVehicleType()
        {
            try
            {
                var requests = await _vehicleRepository.GetVehicleTypeDTOsAsync();
                if(requests != null )
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle Type not found");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Vehicle Typle failed", Details = ex.Message });
            }
            
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("listVehicle")]
        public async Task<IActionResult> GetVehicleList()
        {
            try
            {
                var requests = await _vehicleRepository.GetVehicleDTOsAsync();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle list not found");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Vehicle failed", Details = ex.Message });
            }
        }

        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPost("addVehicle")]
        public async Task<IActionResult> AddVehicle(VehicleAddDTO vehicleAddDTO, string driverName)
        {
            try
            {

                var isAdded = await _vehicleRepository.AddVehicleAsync(vehicleAddDTO, driverName);
                return Ok(new { Message = "Vehicle added successfully.", Vehicle = vehicleAddDTO });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle Add failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("addVehicleByStaff")]
        public async Task<IActionResult> AddVehicleByStaff(int requestID, bool isApprove)
        {
            try
            {
                var responseVehicle = await _vehicleRepository.AddVehicleByStaffcheckAsync(requestID, isApprove);
                if(responseVehicle)
                {
                    return Ok(new { Message = "Vehicle added successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Vehicle addition denied." });
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle By Staff failed", Details = ex.Message });
            }

        }
        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPut("updateVehicle/{id}/{driverName}")]
        public async Task<IActionResult> UpdateVehicle(int id, string driverName)
        {
            try
            {
                var checkUpdate = await _vehicleRepository.UpdateVehicleAsync(id, driverName);
                
                    return Ok(new { Message = "Vehicle Update successfully." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "UpdateVehicle Update failed", Details = ex.Message });
            }

        }
        [Authorize(Roles = "Staff")]
        [HttpDelete("deleteVehicleByStatus/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id)
        {
            try
            {
                var delete = await _vehicleRepository.DeleteVehicleAsync(id);

                if (delete)
                {
                    return Ok(new { Message = "Vehicle delete successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Vehicle delete failed." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "DeleteVehicle Delete failed", Details = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("getStartPointTripFromVehicle/{vehicleId}")]
        public async Task<IActionResult> getStartPointTripFromVehicle(int vehicleId)
        {
            try
            {
                var listStartPoint = await _vehicleRepository.GetListStartPointByVehicleId(vehicleId);   
                if(listStartPoint == null)
                {
                    return NotFound();
                }
                return Ok(listStartPoint);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
        [Authorize]
        [HttpGet("getEndPointTripFromVehicle/{vehicleId}")]
        public async Task<IActionResult> getEndPointTripFromVehicle(int vehicleId)
        {
            try
            {
                var listEndPoint =  await _vehicleRepository.GetListEndPointByVehicleId(vehicleId);
                if (listEndPoint == null)
                {
                    return NotFound();
                }
                return Ok(listEndPoint);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        //[Authorize(Roles = "Staff")]
        [HttpPut("assignDriverForVehicle/")]
        public async Task<IActionResult> AssignDriverForVehicle([FromBody] RentDriverDTO dto)

        {
            try
            {
                var isAssigned = await _vehicleRepository.RegisterDriverAsync(dto);

                if (isAssigned)
                {
                    return Ok(new { Message = "Driver assigned to vehicle successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Assignment failed. Check if vehicle or driver exists." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AssignDriverForVehicle failed", Details = ex.Message });
            }
        }

    }
}
