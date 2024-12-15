using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryRentDriverController : ControllerBase
    {
        private readonly IHistoryRentDriverRepository _historyRentDriverRepository;
        private readonly GetInforFromToken _getInforFromToken;

        public HistoryRentDriverController(IHistoryRentDriverRepository historyRentDriverRepository, GetInforFromToken getInforFromToken)
        {
            _historyRentDriverRepository = historyRentDriverRepository;
            _getInforFromToken = getInforFromToken;
        }

        [HttpGet("ListDriverRent")]
        public async Task<IActionResult> GetDriverUseRent()
        {
            try
            {
                var requests = await _historyRentDriverRepository.GetListHistoryRentDriver();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Driver List Rent not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Driver Rent failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("AddHistoryDriver")]
        public async Task<IActionResult> AddHistoryDriverUseRent(AddHistoryRentDriver add)
        {
            try
            {
                var requests = await _historyRentDriverRepository.AcceptOrDenyRentDriver(add);
                if (requests)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Driver Rent can't be added");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "History Add Driver failed", Details = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("rent-details-with-total-for-owner")]
        public async Task<IActionResult> GetRentDetailsWithTotalForOwner()
        {
            try
            {
                var result = await _historyRentDriverRepository.GetRentDetailsWithTotalForOwner();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch rent details for the owner.", error = ex.Message });
            }
        }

        [HttpPost("/AssignDriverForRent")]
        public async Task<IActionResult> UpdateDriverInRequest(int driverId, int requestId)
        {
            try
            {
                var result = await _historyRentDriverRepository.UpdateDriverInRequestAsync(driverId, requestId);

                if (result)
                {
                    return Ok(new { Message = "Driver updated successfully for the request." });
                }
                else
                {
                    return NotFound(new { Message = "Request not found or update failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update driver for the request.", Details = ex.Message });
            }
        }
        
        [Authorize(Roles = "Staff, VehicleOwner, Driver")]
        [HttpGet("listHistoryRentDriver")]
        public async Task<IActionResult> getHistoryRentDriver()
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
                var role = _getInforFromToken.GetRoleFromToken(token);
                var listHistoryRentDriver = await _historyRentDriverRepository.getHistoryRentDriver(userId, role);
                return Ok(listHistoryRentDriver);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
