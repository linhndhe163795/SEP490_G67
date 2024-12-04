using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.HistoryRentVehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryRentVehicleController : ControllerBase
    {
        private readonly IHistoryRentVehicleRepository _historyRentVehicleRepository;
        private readonly GetInforFromToken _getInforFromToken;

        public HistoryRentVehicleController(IHistoryRentVehicleRepository historyRentVehicleRepository, GetInforFromToken getInforFromToken)
        {
            _historyRentVehicleRepository = historyRentVehicleRepository;
            _getInforFromToken = getInforFromToken;
        }
        //thiếu role
        [HttpGet("listVehicleUseRent")]
        public async Task<IActionResult> GetVehicleUseRent()
        {
            try
            {
                var requests = await _historyRentVehicleRepository.historyRentVehicleListDTOs();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle List Rent not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Vehicle List failed", Details = ex.Message });
            }
        }
        //[HttpGet("listHistoryRentVehicle")]
        //public async Task<IActionResult> getListHistoryRentVehile()
        //{
        //    try
        //    {
        //        string token = Request.Headers["Authorization"];
        //        if (token.StartsWith("Bearer"))
        //        {
        //            token = token.Substring("Bearer ".Length).Trim();
        //        }
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            return BadRequest("Token is required.");
        //        }
        //        var userId = _getInforFromToken.GetIdInHeader(token);
        //        var listHistoryRentVehicel = await _historyRentVehicleRepository.listHistoryRentVehicle(userId);
        //        return Ok(listHistoryRentVehicel);
        //    }catch(Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [Authorize(Roles = "Staff")]
        [HttpPost("AddHistoryVehicle")]
        public async Task<IActionResult> AddHistoryVehicleUseRent(AddHistoryVehicleUseRent add)
        {
            try
            {
                var requests = await _historyRentVehicleRepository.AccpetOrDeninedRentVehicle(add);
                if (requests)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle Rent can't addd");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "History Add Vehicle failed", Details = ex.Message });
            }

        }
    }
}
