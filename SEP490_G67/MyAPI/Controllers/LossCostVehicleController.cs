using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LossCostVehicleController : ControllerBase
    {
        private readonly ILossCostVehicleRepository _lossCostVehicleRepository;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IMapper _mapper;
        public LossCostVehicleController(ILossCostVehicleRepository lossCostCarRepository, IMapper mapper, GetInforFromToken getInforFromToken)
        {
            _lossCostVehicleRepository = lossCostCarRepository;
            _getInforFromToken = getInforFromToken;
            _mapper = mapper;
        }
        [Authorize(Roles = "Staff,VehicleOwner")]
        [HttpGet("lossCostCar/vehicleId/startDate/endDate")]
        public async Task<IActionResult> lossCostVehicleByDate(int? vehicleId, DateTime? startDate, DateTime? endDate, int? vehicleOwnerId)
        {
            try
            {
                var listLossCost = await _lossCostVehicleRepository.GetAllLostCost();
                return Ok(listLossCost);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("updateLossCost/id")]
        public async Task<IActionResult> updateLossCostById(int id, LossCostUpdateDTO lossCostupdateDTOs)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                await _lossCostVehicleRepository.UpdateLossCostById(id, lossCostupdateDTOs, userId);
                return Ok(lossCostupdateDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest("updateLossCostById: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("addLossCostVehicle")]
        public async Task<IActionResult> addLossCost(LossCostAddDTOs lossCostAddDTOs)
        {
            string token = Request.Headers["Authorization"];
            if (token.StartsWith("Bearer"))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }
            var userId = _getInforFromToken.GetIdInHeader(token);
            try
            {
                await _lossCostVehicleRepository.AddLossCost(lossCostAddDTOs, userId);
                return Ok(lossCostAddDTOs);
            }
            catch (Exception ex)
            {
                throw new Exception("addLossCost: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("deleteLossCost/id")]
        public async Task<IActionResult> deleteCostById(int id)
        {
            try
            {
                await _lossCostVehicleRepository.DeleteLossCost(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("deleteCostById: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpGet("totalLossVehicel")]
        public async Task<IActionResult> getTotalLossVehicle()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var result = await _lossCostVehicleRepository.GetLossCostVehicleByDate(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("totalLossVehicel/startDate/endDate/vehicleId")]
        public async Task<IActionResult> getTotalLossVehicleUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var result = await _lossCostVehicleRepository.GetLossCostVehicleByDateUpdate(startDate, endDate, vehicleId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
