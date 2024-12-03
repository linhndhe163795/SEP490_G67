using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryRentDriverController : ControllerBase
    {
        private readonly IHistoryRentDriverRepository _historyRentDriverRepository;

        public HistoryRentDriverController(IHistoryRentDriverRepository historyRentDriverRepository)
        {
            _historyRentDriverRepository = historyRentDriverRepository;
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
        public async Task<IActionResult> GetRentDetailsWithTotalForOwner([FromQuery] DateTime startDate,[FromQuery] DateTime endDate , int? vehicleId, int? vehicleOwnerId)
        {
            try
            {
                // Gọi phương thức repository để lấy thông tin chi tiết các lần thuê và tổng chi phí
                var result = await _historyRentDriverRepository.GetRentDetailsWithTotalForOwner(startDate, endDate, vehicleId,vehicleOwnerId);

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
                // Gọi hàm repository để cập nhật driverId cho requestId
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
        [Authorize(Roles = "Driver")]
        [HttpGet("GetHistoryRentDriverByDriverId")]
        public async Task<IActionResult> GetDriverHistory()
        {
            try
            {
                var history = await _historyRentDriverRepository.GetDriverHistoryByUserIdAsync();

                if (history == null || !history.Any())
                {
                    return NotFound($"No history found for driver with ID");
                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch driver history.", error = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("GetHistoryRentDriverForStaff")]
        public async Task<IActionResult> GetDriverRentInfo([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var rentInfo = await _historyRentDriverRepository.GetDriverRentInfo(startDate, endDate);

                if (rentInfo == null || !rentInfo.Any())
                {
                    return NotFound(new { Message = "No rent info found for the specified criteria." });
                }

                return Ok(rentInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch driver rent info.", Details = ex.Message });
            }
        }
        [Authorize(Roles = "VehicleOwner")]
        [HttpGet("GetHistoryRentDriverForVehicleOwner")]
        public async Task<IActionResult> GetHistoryByVehicleOwner()
        {
            try
            {
                var historyList = await _historyRentDriverRepository.GetHistoryByVehicleOwnerAsync();

                if (historyList == null || !historyList.Any())
                {
                    return NotFound(new { Message = "No history found for the vehicle owner." });
                }

                return Ok(historyList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch history by vehicle owner.", Details = ex.Message });
            }
        }

    }
}
